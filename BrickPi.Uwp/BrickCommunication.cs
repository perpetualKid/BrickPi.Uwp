//#define DEBUGMESSAGES

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BrickPi.Uwp.Base;
using Windows.Storage.Streams;

namespace BrickPi.Uwp
{
    public sealed partial class Brick : IDisposable
    {
        private readonly TimeSpan ReadTimeout = new TimeSpan(TimeSpan.TicksPerMillisecond / 10 * 75); //7.5ms
        private readonly TimeSpan WriteTimeout = new TimeSpan(TimeSpan.TicksPerMillisecond / 10 * 25); //2.5ms
        private readonly TimeSpan CancelAfter = new TimeSpan(TimeSpan.TicksPerMillisecond * 500); //50 ms

        //https://github.com/Microsoft/Windows-universal-samples/blob/master/Samples/CustomSerialDeviceAccess/cs/Scenario3_ReadWrite.xaml.cs

        // Track Read Operation 
        private CancellationTokenSource readCancellationTokenSource;
        private object readCancelLock = new Object();
        private uint readBytesCounter;

        // Track Write Operation 
        private CancellationTokenSource writeCancellationTokenSource;
        private object writeCancelLock = new Object();
        private uint writeBytesCounter;

        SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private DataReader dataReaderObject;
        private DataWriter dataWriterObject;


        private CancellationTokenSource handlerCancellationTokenSource;
        private object handlerCancelLock = new Object();
        private SemaphoreSlim handlerSemaphore = new SemaphoreSlim(1, 1);

        /*

        Sending byte array looks like this:

        RPi to BrickPi:  
        byte   name         description
        0      DEST_ADDR    Destination address for the message, or 0 for BROADCAST.
        1      CHECKSUM     Checksum computed across the entire message.
        2      BYTE_COUNT   The count of bytes in the message body, excluding the header.
        3-n                 The data

        BrickPi to RPi (no need for address):
        byte   name         description
        0      CHECKSUM     Checksum computed across the entire message.
        1      BYTE_COUNT   The count of bytes in the message body, excluding the header.
        2-n                 The data  

        Returned values

        -6 wrong message length
        -5 wrong checksum
        -4 not even the entire header was received
        -3 not my address
        -2 timeout           
        -1 something went wrong
        0  Destination address was BROADCAST
        1  Destination address was mine

        */

        #region public methods
        /// <summary>
        /// Returns the Brick Version. Should be 2 on current BrickPi firmware.
        /// Always good to check first in code.
        /// Note: this resets any sensor information
        /// </summary>
        /// <returns>Brick Version</returns>
        public async Task<int> GetBrickVersion()
        {
            try
            {
                CancelHandlerTask();
                await handlerSemaphore.WaitAsync().ConfigureAwait(false);

                foreach (var sensor in sensors)
                    sensor.SensorType = SensorType.FW_VERSION;
                await BrickPiInitializeSensors().ConfigureAwait(false);
                await BrickPiUpdateValues().ConfigureAwait(false);
                foreach (var sensor in sensors)
                    sensor.SensorType = SensorType.RAW;
                return sensors[SensorPort.Port_S4].RawValue;
            }
            finally
            {
                handlerSemaphore.Release();
            }
        }

        public async Task<bool> SetTimeout(int timeout)
        {
            try
            {
                CancelHandlerTask();
                await handlerSemaphore.WaitAsync().ConfigureAwait(false);
                return await BrickPiSetTimeout(timeout).ConfigureAwait(false);
            }
            finally
            {
                handlerSemaphore.Release();
            }
        }

        public async Task<bool> InitializeSensors()
        {
            try
            {
                CancelHandlerTask();
                await handlerSemaphore.WaitAsync().ConfigureAwait(false);
                return await BrickPiInitializeSensors().ConfigureAwait(false);
            }
            finally
            {
                handlerSemaphore.Release();
            }
        }

        public async Task<bool> UpdateValues()
        {
            try
            {
                CancelHandlerTask();
                await handlerSemaphore.WaitAsync().ConfigureAwait(false);
                return await BrickPiUpdateValues().ConfigureAwait(false);
            }
            finally
            {
                handlerSemaphore.Release();
            }
        }


        /// <summary>
        /// Start reading the brick, getting info from sensors, moving motors
        /// </summary>
        public void Start()
        {
            CancelHandlerTask();
            Task.Run(async () => await ContinuousUpdate(handlerCancellationTokenSource.Token), handlerCancellationTokenSource.Token).ConfigureAwait(false);
        }


        /// <summary>
        /// Stop reading the brick, getting info from sensors, moving motors
        /// </summary>
        public async Task Stop()
        {
            try
            {
                CancelHandlerTask();
                await handlerSemaphore.WaitAsync().ConfigureAwait(false);
            }
            finally
            {
                handlerSemaphore.Release();
            }
        }
        #endregion

        #region private implementation
        private async Task<bool> BrickPiInitializeSensors()
        {
            bool result = true;

            foreach (Arduino arduino in EnumExtension<Arduino>.All())
            {
                ProtocolArray commandData = new ProtocolArray();
                TimeSpan? setupTime = null;
                int retry = 0;
                commandData[Const.MessageTypeIndex] = (byte)MessageType.ChangeSensorType;
                foreach (SensorPort sensorPort in arduino.SensorPorts())
                {
                    setupTime = (sensors[sensorPort].InitializeSensorRequest(commandData).Max(setupTime));
                }

                while (retry++ < 3)
                {
                    int bytes = 3 + commandData.Bytes;
#if DEBUGMESSAGES
                    Debug.WriteLine("BrickPiInitializeSensors - " + arduino);
#endif 
                    byte[] resultData = await BrickPiTxAndRx(arduino, bytes, commandData.Data, null, setupTime).ConfigureAwait(false);
                    if (null != resultData && resultData.Length == 1 && ((MessageType)resultData[Const.MessageTypeIndex] == MessageType.ChangeSensorType))
                    {
                        result &= true;
                        break;
                    }
                    Debug.WriteLine("Trying again to setup sensors on Arduino " + arduino.ToString());
                    await Task.Delay(100).ConfigureAwait(false);
                }
            }
            return result;
        }

        /// <summary>
        /// Set a new motor timeout on the brick
        /// </summary>
        private async Task<bool> BrickPiSetTimeout(int timeout)
        {
            ProtocolArray dataArray = new ProtocolArray(5);
            bool result = true;
            foreach (Arduino arduino in EnumExtension<Arduino>.All())
            {
                dataArray[Const.MessageTypeIndex] = (byte)MessageType.ChangeTimeout;
                Array.Copy(BitConverter.GetBytes(timeout), 0, dataArray.Data, Const.TimeoutIndex, 4);
#if DEBUGMESSAGES
                Debug.WriteLine("BrickPiSetTimeout - " + arduino);
#endif 
                byte[] resultData = await BrickPiTxAndRx(arduino, 5, dataArray.Data).ConfigureAwait(false);
                result &= (null != resultData && resultData.Length == 1 && ((MessageType)resultData[Const.MessageTypeIndex] == MessageType.ChangeTimeout));
            }
            return result;
        }

        /// <summary>
        /// Used to change the address of the Arduino
        /// </summary>
        /// <param name="currentAddress">current address</param>
        /// <param name="newAddress">new address</param>
        /// <returns></returns>
        private async Task<bool> BrickPiChangeAddress(Arduino arduino, byte newAddress)
        {
            ProtocolArray dataArray = new ProtocolArray();
            dataArray[Const.MessageTypeIndex] = (int)MessageType.ChangeAddress;
            dataArray[Const.NewAddressIndex] = newAddress;
#if DEBUGMESSAGES
            Debug.WriteLine("BrickPiChangeAddress - " + arduino);
#endif 
            byte[] result = await BrickPiTxAndRx(arduino, 2, dataArray.Data).ConfigureAwait(false);

            return (result != null && result.Length > 1 && (MessageType)result[Const.MessageTypeIndex] == MessageType.ChangeAddress);
        }


        private async Task ContinuousUpdate(CancellationToken cancellationToken)
        {
            try
            {
                await handlerSemaphore.WaitAsync().ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                while (!cancellationToken.IsCancellationRequested)
                {
                    await BrickPiUpdateValues().ConfigureAwait(false);
                    await Task.Delay(20, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                handlerSemaphore.Release();
            }
        }


        /// <summary>
        /// poll the BrickPi for updates on sensor and encoder data, and send motor command
        /// </summary>
        /// <returns></returns>
        private async Task<bool> BrickPiUpdateValues()
        {
            ProtocolArray dataArray;
            int retry = 0;

            foreach (Arduino arduino in EnumExtension<Arduino>.All())
            {
                //Fill the header of buffer for communication
                dataArray = new ProtocolArray();
                dataArray[Const.MessageTypeIndex] = (int)MessageType.Datagram;

                // motors encoder offset
                foreach (MotorPort motorPort in arduino.MotorPorts())
                {
                    motors[motorPort].UpdateEncoderRequest(dataArray);
                }

                // motors speed and direction
                foreach (MotorPort motorPort in arduino.MotorPorts())
                {
                    motors[motorPort].UpdateVelocityRequest(dataArray);
                }

                // sensors
                foreach (SensorPort sensorPort in arduino.SensorPorts())
                {
                    sensors[sensorPort].UpdateSensorRequest(dataArray);
                }

                int bytes = 1 + dataArray.Bytes;
#if DEBUGMESSAGES
                Debug.WriteLine("BrickPiUpdateValues - " + arduino);
#endif 

                byte[] resultData = await BrickPiTxAndRx(arduino, bytes, dataArray.Data).ConfigureAwait(false);
                if (resultData == null)
                    continue;

                if (resultData == null || resultData.Length <= 1 || (MessageType)resultData[Const.MessageTypeIndex] != MessageType.Datagram)
                {
                    Debug.WriteLine(string.Format("Error Updating values: {0}", BitConverter.ToString(resultData ?? System.Text.Encoding.Unicode.GetBytes("No Data Received"))));
                    await Task.Delay(100).ConfigureAwait(false);
                    if (retry++ < 3)
                    {
                        continue;
                    }
                    else
                        return false;
                }
                dataArray = new ProtocolArray(resultData);

                // motors
                // need to get number of bits to be used for each encoder first
                byte[] encoderBits = new byte[] { (byte)dataArray.GetBits(1, 5), (byte)dataArray.GetBits(1, 5) };
                foreach (MotorPort motorPort in arduino.MotorPorts())
                {
                    motors[motorPort].UpdateResponse(dataArray, encoderBits[(byte)motorPort % 2]);
                }

                // sensors
                foreach (SensorPort sensorPort in arduino.SensorPorts())
                {
                    sensors[sensorPort].UpdateSensorResponse(dataArray);
                }
            }
            return true;
        }
        #endregion

        #region Serial Communication
        private async Task<byte[]> BrickPiTxAndRx(Arduino arduino, int bytesSend, byte[] sendBuffer,
            TimeSpan? writeTimeout = null, TimeSpan? readTimeout = null, TimeSpan? cancelAfter = null)
        {
            try
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                await BrickPiTx(arduino, bytesSend, sendBuffer, writeTimeout, cancelAfter).ConfigureAwait(false);
                Util.SpinUntil(WriteTimeout);
                return await BrickPiRx(readTimeout, cancelAfter).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        }


        /// <summary>
        /// Send data to the BrickPi
        /// </summary>
        /// <param name="dest">The target Arduino, can be 1 or 2</param>
        /// <param name="byteCount">number of bytes to send</param>
        /// <param name="OutArray">byte array to send</param>
        private async Task BrickPiTx(Arduino target, int byteCount, byte[] buffer, TimeSpan? timeout = null, TimeSpan? cancelAfter = null)
        {
            if (byteCount > buffer.Length)
                return;
            timeout = timeout ?? WriteTimeout;
            cancelAfter = cancelAfter ?? CancelAfter;
            byte[] sendBuffer = new byte[3 + byteCount];
            sendBuffer[0] = (byte)target;
            sendBuffer[1] = (byte)(((byte)target + byteCount + CrossSum(0, byteCount, buffer)) % 256);
            sendBuffer[2] = (byte)byteCount;
            Array.Copy(buffer, 0, sendBuffer, 3, byteCount);
#if DEBUGMESSAGES
            Debug.WriteLine("Sending: {0}", new object[] { BitConverter.ToString(sendBuffer, 0, sendBuffer.Length) });
#endif
            try
            {
                // clean token
                ResetWriteCancellationTokenSource();
                writeCancellationTokenSource.CancelAfter(cancelAfter.Value.Add(timeout.Value));
                //Launch the WriteAsync task to perform the write
                await WriteAsync(timeout.Value, writeCancellationTokenSource.Token, sendBuffer).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error sending data: {0}", ex.Message));
                // Cleanup once complete
                if (dataWriterObject != null)
                {
                    dataWriterObject.DetachStream();
                    dataWriterObject = null;
                }
            }
        }

        /// <summary>
        /// Read data coming from the BrickPi
        /// </summary>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns></returns>
        private async Task<byte[]> BrickPiRx(TimeSpan? timeout = null, TimeSpan? cancelAfter = null)
        {
            timeout = timeout ?? ReadTimeout;
            cancelAfter = cancelAfter ?? CancelAfter;
            try
            {
                ResetReadCancellationTokenSource();
                readCancellationTokenSource.CancelAfter(cancelAfter.Value.Add(timeout.Value));
                //read from the serial port
                byte[] buffer = await ReadAsync(timeout.Value, readCancellationTokenSource.Token).ConfigureAwait(false);

                // check if data are valids
                if ((buffer == null) || (buffer.Length < 2) || (buffer.Length < (buffer[1] + 2)))
                    return null;
#if DEBUGMESSAGES
                Debug.WriteLine("Receiving: {0}", new object[] { BitConverter.ToString(buffer) });
#endif
                long checkSum = (CrossSum(1, buffer.Length - 1, buffer) % 256);
                if (checkSum != buffer[0])
                {
                    Debug.WriteLine("Checksum Error on input stream.");
                    return null;
                }
                byte[] result = new byte[buffer[1]];
                Array.Copy(buffer, 2, result, 0, buffer[1]);
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error receiving data: {0}", ex.Message));
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
                return null;
            }
        }

        /// <summary>
        /// ReadAsync: Task that waits on data and reads asynchronously from the serial device InputStream
        /// </summary>
        /// <param name="cancellationToken">cancellation token for async read</param>
        /// <returns></returns>
        private async Task<byte[]> ReadAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;
            uint bufferSize = 256;

            // Don't start any IO if we canceled the task
            lock (readCancelLock)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (dataReaderObject == null)
                    dataReaderObject = new DataReader(serialPort.InputStream);
                // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
                dataReaderObject.InputStreamOptions = InputStreamOptions.ReadAhead;
                // set serial timeout for reading. initialize the timout reading function
                serialPort.ReadTimeout = timeout; //ReadTimeout doesn't seem to work. See also http://stackoverflow.com/questions/32157918/unable-to-use-serialdevice-readtimeout-in-windows-10-iot
                // Create a task object to wait for data on the serialPort.InputStream
                loadAsyncTask = dataReaderObject.LoadAsync(bufferSize).AsTask(cancellationToken);
                
            }
            //// Launch the task and wait
            UInt32 bytesRead = await loadAsyncTask.ConfigureAwait(false);

            byte[] result = null;
            if (bytesRead > 0)
            {
                result = new byte[bytesRead];
                dataReaderObject.ReadBytes(result); //return the bytes read
                readBytesCounter += bytesRead;
            }
            else
                Debug.WriteLine(String.Format("No bytes received"));
            return result;
        }

        private async Task WriteAsync(TimeSpan timeout, CancellationToken cancellationToken, byte[] buffer)
        {

            Task<UInt32> storeAsyncTask;

            // Don't start any IO if we canceled the task 
            lock (writeCancelLock)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (dataWriterObject == null)
                    dataWriterObject = new DataWriter(serialPort.OutputStream);

                dataWriterObject.WriteBytes(buffer);

                // set serial timeout for writing. initialize the timout writing function
                serialPort.WriteTimeout = timeout;
                // Cancellation Token will be used so we can stop the task operation explicitly 
                // The completion function should still be called so that we can properly handle a canceled task 
                storeAsyncTask = dataWriterObject.StoreAsync().AsTask(cancellationToken);
            }

            UInt32 bytesWritten = await storeAsyncTask.ConfigureAwait(false);

            if (bytesWritten > 0)
            {
                writeBytesCounter += bytesWritten;
            }
            else
                Debug.WriteLine("Error sending data");
        }
        #endregion

        private void CancelIoTasks()
        {
            CancelReadTask();
            CancelWriteTask();
        }

        #region checksum
        /// <summary>
        /// checksum calculator
        /// </summary>
        /// <param name="startIndex">start index</param>
        /// <param name="byteCount">number of bytes</param>
        /// <param name="array">byte array</param>
        /// <returns></returns>
        private static long CrossSum(int startIndex, int byteCount, byte[] array)
        {
            //check errors
            if ((startIndex < 0) || ((startIndex + byteCount) > array.Length))
                return long.MaxValue;

            //run the Cross Sum
            long result = 0;
            byteCount += startIndex;
            for (int i = startIndex; i < byteCount; i++)
            {
                result += array[i];
            }
            return result;
        }
        #endregion

        #region Async Cancellation Helper
        private void CancelHandlerTask()
        {
            lock (handlerCancelLock)
            {
                if ((handlerCancellationTokenSource != null) && (!handlerCancellationTokenSource.IsCancellationRequested))
                {
                    handlerCancellationTokenSource.Cancel();
                    // Existing IO already has a local copy of the old cancellation token so this reset won't affect it 
                    ResetHandlerCancellationTokenSource();
                }
            }
        }
        private void CancelReadTask()
        {
            lock (readCancelLock)
            {
                if ((readCancellationTokenSource != null) && (!readCancellationTokenSource.IsCancellationRequested))
                {
                    readCancellationTokenSource.Cancel();
                    // Existing IO already has a local copy of the old cancellation token so this reset won't affect it 
                    ResetReadCancellationTokenSource();
                }
            }
        }


        private void CancelWriteTask()
        {
            lock (writeCancelLock)
            {
                if ((writeCancellationTokenSource != null) && (!writeCancellationTokenSource.IsCancellationRequested))
                {
                    writeCancellationTokenSource.Cancel();
                    // Existing IO already has a local copy of the old cancellation token so this reset won't affect it 
                    ResetWriteCancellationTokenSource();
                }
            }
        }


        private void ResetHandlerCancellationTokenSource()
        {
            if (handlerCancellationTokenSource != null)
            {
                handlerCancellationTokenSource.Dispose();
            }
            // Create a new cancellation token source so that can cancel all the tokens again 
            handlerCancellationTokenSource = new CancellationTokenSource();

            // Hook the cancellation callback (called whenever Task.cancel is called) 
            //handlerCancellationTokenSource.Token.Register(() => NotifyHandlerCancelingTask());

        }

        private void ResetReadCancellationTokenSource()
        {
            if (readCancellationTokenSource != null)
            {
                readCancellationTokenSource.Dispose();
            }
            // Create a new cancellation token source so that can cancel all the tokens again 
            readCancellationTokenSource = new CancellationTokenSource();

            // Hook the cancellation callback (called whenever Task.cancel is called) 
            //readCancellationTokenSource.Token.Register(() => NotifyReadCancelingTask()); 
        }


        private void ResetWriteCancellationTokenSource()
        {
            if (writeCancellationTokenSource != null)
            {
                writeCancellationTokenSource.Dispose();
            }
            // Create a new cancellation token source so that can cancel all the tokens again 
            writeCancellationTokenSource = new CancellationTokenSource();

            // Hook the cancellation callback (called whenever Task.cancel is called) 
            //writeCancellationTokenSource.Token.Register(() => NotifyWriteCancelingTask());

        }
        #endregion

        #region IDisposable Support
        private bool disposed = false; 

        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (readCancellationTokenSource != null)
                    {
                        readCancellationTokenSource.Cancel();
                        readCancellationTokenSource.Dispose();
                        readCancellationTokenSource = null;
                    }

                    if (writeCancellationTokenSource != null)
                    {
                        writeCancellationTokenSource.Cancel();
                        writeCancellationTokenSource.Dispose();
                        writeCancellationTokenSource = null;
                    }
                    if (handlerCancellationTokenSource != null)
                    {
                        handlerCancellationTokenSource.Cancel();
                        handlerCancellationTokenSource.Dispose();
                        handlerCancellationTokenSource = null;
                    }
                    if (serialPort != null)
                    {
                        serialPort.Dispose();
                        serialPort = null;
                    }
                    if (semaphore != null)
                    {
                        semaphore.Dispose();
                        semaphore = null;
                    }
                    if (null != handlerSemaphore)
                    {
                        handlerSemaphore.Dispose();
                        handlerSemaphore = null;
                    }
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }

}
