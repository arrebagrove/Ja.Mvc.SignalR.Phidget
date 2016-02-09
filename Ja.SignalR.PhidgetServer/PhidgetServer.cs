using Microsoft.AspNet.SignalR.Client;
using System;
using System.IO;
using System.Threading.Tasks;
using Ja.SignalR.Phidget.Models;
using System.Net;
using System.Net.Sockets;
using Phidgets;
using Phidgets.Events;

namespace Ja.SignalR.PhidgetServer
{
    public class PhidgetServer
    {
        private TextWriter _traceWriter;
        private HubConnection _hubConnection;
        private IHubProxy _phidgetServerHubProxy;
        /// <summary>
        /// Can be PhidgetRFIDModule, PhidgetServoModule or PhidgetStarterKitModule.
        /// Maintain Phidget hardware properties state for all PhidgetClient (static field).
        /// </summary>
        private static PhidgetModuleProperties _phidgetData;
        private static SignalRClientDetails _phidgetServerDetails;
        private static string _phidgetModuleType;
        private static PhidgetModule _phidgetModule;
        public IHubProxy PhidgetServerHubProxy
        {
            get
            {
                return _phidgetServerHubProxy;
            }

            set
            {
                _phidgetServerHubProxy = value;
            }
        }

        public PhidgetServer(TextWriter traceWriter, string phidgetModuleType)
        {
            _traceWriter = traceWriter;
             // Which PhidgetModule use this PhidgetServer ?
            _phidgetModuleType = phidgetModuleType;
        }
        /// <summary>
        /// Main Task.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task RunAsync(string url)
        {
            try
            {
                await RunPhidgetServerAsync(url);
            }
            catch (HttpClientException httpClientException)
            {
                Console.WriteLine("HttpClientException: {0}", httpClientException.Response);
                throw;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception: {0}", exception);
                throw;
            }
        }
        /// <summary>
        /// PhidgetServer Child Task.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task RunPhidgetServerAsync(string url)
        {

            Console.WriteLine("Starting PhidgetServer signalR client for:  " + url);

            _hubConnection = new HubConnection(url);
            _hubConnection.TraceWriter = _traceWriter;
            _hubConnection.TraceLevel = TraceLevels.All;

            PhidgetServerHubProxy = _hubConnection.CreateHubProxy("PhidgetMiddlewareServerHub");

            Init();

            try
            {
                await _hubConnection.Start();
                Console.WriteLine("Transport.Name={0} // ConnectionID={1}", _hubConnection.Transport.Name, _hubConnection.ConnectionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
                Console.WriteLine("END Bye");
                Console.ReadKey();
                return;
            }
            // Complete PhidgetServer informations with ConnectionId.
            _phidgetServerDetails.ConnectionId = _hubConnection.ConnectionId;

            // After connection, PhidgetServer and PhidgetClient must call AfterConnect.
            Console.WriteLine(string.Format("AfterConnect: {0} / {1}", _phidgetServerDetails.MachineName, _phidgetServerDetails.SignalRClientTypeConnected));
            await PhidgetServerHubProxy.Invoke("AfterConnect", _phidgetServerDetails).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("!!! There was an error on AfterConnect call:{0} \n", task.Exception.GetBaseException());
                }
            });
            // Infinit loop.
            while (true)
            {
                Console.WriteLine("Q to QUIT");

                string key = Console.ReadLine();
                if (key.ToUpper() == "Q")
                {
                    break;
                }
            }
            // Terminate service.
            Terminate();

            Console.ReadKey();
        }
        /// <summary>
        /// We register first server RPC methods  for signalr clients,
        /// then we initialize Phidget hardware depending on phidgetModuleType,
        /// then we retreive initial phidget data depending on phidgetModuleType,
        /// finally we initialize PhidgetServer informations (name, IP ...).
        /// </summary>
        private void Init()
        {
            RegisterPhidgetServerMethods();
            _phidgetModule = new PhidgetModule(this);
            InitPhidgetServerInformations();
        }
        /// <summary>
        /// Close phidget hardware and hub connection.
        /// </summary>
        public void Terminate()
        {
            // Close Phidget.
            try
            {
                _phidgetModule.ClosePhidgetHardware();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            // On "PS disconnect".
            // This will call: OnDisconnected(bool stopCalled).
            _hubConnection.Stop();
            Console.WriteLine("END Bye");
        }
        /// <summary>
        /// SignalRClient of type PhidgetServer RPC registration.
        /// </summary>
        private void RegisterPhidgetServerMethods()
        {
            //
            // PhidgetServer RPC registration.
            // http://stackoverflow.com/questions/29854027/signalr-issue-with-invoking-server-method-from-onreceived-method
            // On PhidgetMiddlewareServer (PMS) call get.
            // We have to retreive Phidget data and call SendToPhidgetClient(PCxID, data).
            PhidgetServerHubProxy.On<string>("get", async (string x) =>
            {
                Console.WriteLine("PhidgetClient ID = " + x);
                // Retreive latest PhidgetServer data and store in _phidgetData. (for me dont need to do that)
                _phidgetModule.GetPhidgetServerData();

                // Call SendToPhidgetClient(PCxID, data).
                Console.WriteLine(string.Format("SendToPhidgetClient {0} / {1}", x, _phidgetData.Name));
                //await Task.Run(async () => {
                //    _phidgetServerHubProxy.Invoke("SendToPhidgetClient", x, _phidgetData).ContinueWith(task =>
                //{
                //    if (task.IsFaulted)
                //    {
                //        Console.WriteLine("!!! There was an error on SendToPhidgetClient call:{0} \n", task.Exception.GetBaseException());
                //    }
                //}).Wait();

                await Task.Run(async () =>
                {
                    await PhidgetServerHubProxy.Invoke("SendToPhidgetClient", x, _phidgetData);
                }).ConfigureAwait(false);//this will run the task to seperate thread, unblocking the parent thread
            });

            // On PhidgetMiddlewareServer (PMS) call set.
            // We have to set Phidget data and call SendToPhidgetClients(PSx, data).
            //_phidgetServerHubProxy.On<PhidgetModuleProperties>("set", x =>
            //{
            //    Console.WriteLine(x.Name);
            //    // Set PhidgetServer data stored in _phidgetData.
            //    _phidgetData = x;
            //    SetPhidgetServerData();

            //    // Call SendToPhidgetClients(PSx, data).
            //    Console.WriteLine(string.Format("SendToPhidgetClients: {0} / {1}", _phidgetServerDetails.MachineName, _phidgetData.ToString()));
            //    _phidgetServerHubProxy.Invoke("SendToPhidgetClients", _phidgetServerDetails , _phidgetData).ContinueWith(task =>
            //    {
            //        if (task.IsFaulted)
            //        {
            //            Console.WriteLine("!!! There was an error on SendToPhidgetClients call:{0} \n", task.Exception.GetBaseException());
            //        }
            //    }).Wait();

            //});
            // We cannot use abstract type PhidgetModuleProperties because
            // in signalr abstract type serialization/deserialization thrown an exception
            // Cannot serialize/deserialize abstract type an interface.
            // So we use dynamic type.
            PhidgetServerHubProxy.On<dynamic>("set", async (dynamic x) =>
            {
                Console.WriteLine(x.Name);
                // Set PhidgetServer data stored in x.
                _phidgetModule.SetPhidgetServerData(x);

                // Call SendToPhidgetClients(PSx, data).
                Console.WriteLine(string.Format("SendToPhidgetClients: {0} / {1}", _phidgetServerDetails.MachineName, _phidgetData.ToString()));
                await Task.Run(async () =>
                {
                    await PhidgetServerHubProxy.Invoke("SendToPhidgetClients", _phidgetServerDetails, _phidgetData);
                }).ConfigureAwait(false);//this will run the task to seperate thread, unblocking the parent thread
            });

        }
        /// <summary>
        /// Init PhidgetServer Informations.
        /// </summary>
        private void InitPhidgetServerInformations()
        {
            Console.WriteLine("InitPhidgetServerInformations");

            _phidgetServerDetails = new SignalRClientDetails
            {
                MachineName = System.Environment.MachineName,
                IPAddress = GetMyIpAddress(),
                SignalRClientTypeConnected = SignalRClientType.PhidgetServer,
                PhidgetModuleTypeAttached = _phidgetModuleType,
            };
        }
        /// <summary>
        /// Get host IpAddress where Phidget module is attached.
        /// </summary>
        /// <returns></returns>
        private string GetMyIpAddress()
        {
            Console.WriteLine("GetMyIpAddress");

            IPHostEntry SystemAC = Dns.GetHostEntry(Dns.GetHostName());
            string IPAddress = string.Empty;

            foreach (var address in SystemAC.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    IPAddress = address.ToString();
                }
            }
            return IPAddress;

        }

        private class PhidgetModule
        {
            private static RFID _rfid; //Declare an RFID object
            private static Servo _servo; //Declare an Servo object
            private static InterfaceKit _ifKit; //Declare an InterfaceKit object
            private PhidgetServer _phidgetServer; //Parent object

            public PhidgetModule(PhidgetServer phidgetServer)
            {
                _phidgetServer = phidgetServer;
                InitPhidgetHardware();
                GetPhidgetServerData();
            }
            /// <summary>
            /// Phidget hardware initialization depending on phidgetModuleType.
            /// </summary>
            public void InitPhidgetHardware()
            {
                switch (_phidgetModuleType)
                {
                    case PhidgetModuleType.PhidgetRFID:
                        rfidInitPhidgetHardware();
                        _phidgetData = new PhidgetRFIDModule(_rfid);
                        break;
                    case PhidgetModuleType.PhidgetServo:
                        servoInitPhidgetHardware();
                        _phidgetData = new PhidgetServoModule(_servo);
                        break;
                    case PhidgetModuleType.PhidgetStarterKit:
                        ifKitInitPhidgetHardware();
                        _phidgetData = new PhidgetStarterKitModule(_ifKit);
                        break;
                    default:
                        break;
                }
            }
            /// <summary>
            /// Close Phidget hardware depending on phidgetModuleType.
            /// </summary>
            public void ClosePhidgetHardware()
            {
                switch (_phidgetModuleType)
                {
                    case PhidgetModuleType.PhidgetRFID:
                        rfidClosePhidgetHardware();
                        break;
                    case PhidgetModuleType.PhidgetServo:
                        servoClosePhidgetHardware();
                        break;
                    case PhidgetModuleType.PhidgetStarterKit:
                        ifKitClosePhidgetHardware();
                        break;
                    default:
                        break;
                }
            }
            /// <summary>
            /// Read PhidgetServer Data, and refresh _phidgetData content depending on phidgetModuleType.
            /// </summary>
            public void GetPhidgetServerData()
            {
                Console.WriteLine("GetPhidgetServerData");

                switch (_phidgetModuleType)
                {
                    case PhidgetModuleType.PhidgetStarterKit:
                        // Read specific data.
                        PhidgetStarterKitModule pdsk = (PhidgetStarterKitModule)_phidgetData;
                        _phidgetData = pdsk.GetData(pdsk);
                        _phidgetData.PhidgetModuleType = _phidgetModuleType;
                        break;
                    case PhidgetModuleType.PhidgetRFID:
                        // Read specific data.
                        _phidgetData.GetData((PhidgetRFIDModule)_phidgetData);
                        _phidgetData.PhidgetModuleType = _phidgetModuleType;
                        break;
                    case PhidgetModuleType.PhidgetServo:
                        // Read specific data.
                        PhidgetServoModule pds = (PhidgetServoModule)_phidgetData;
                        _phidgetData = pds.GetData(pds);
                        _phidgetData.PhidgetModuleType = _phidgetModuleType;
                        break;
                    default:
                        break;
                }
            }
            /// <summary>
            /// Write PhidgetServer data on hardware and refresh _phidgetData content depending on phidgetModuleType.
            /// </summary>
            public void SetPhidgetServerData(dynamic data)
            {
                Console.WriteLine("SetPhidgetServerData");

                switch (_phidgetModuleType)
                {
                    case PhidgetModuleType.PhidgetStarterKit:
                        // Write specific data with _phidgetData 
                        _phidgetData.SetData(data, _ifKit);
                        break;
                    case PhidgetModuleType.PhidgetRFID:
                        // Write specific data with _phidgetData 
                        _phidgetData.SetData(data, _rfid);
                        break;
                    case PhidgetModuleType.PhidgetServo:
                        // Write specific data with _phidgetData 
                        _phidgetData.SetData(data, _servo);
                        break;
                    default:
                        break;
                }

            }

            #region PhidgetModuleType.PhidgetRFID
            public void rfidInitPhidgetHardware()
            {
                try
                {
                    _rfid = new RFID();

                    //initialize our Phidgets RFID reader and hook the event handlers
                    _rfid.Attach += new AttachEventHandler(rfid_Attach);
                    _rfid.Detach += new DetachEventHandler(rfid_Detach);
                    _rfid.Error += new Phidgets.Events.ErrorEventHandler(rfid_Error);
                    _rfid.Tag += new TagEventHandler(rfid_Tag);
                    _rfid.TagLost += new TagEventHandler(rfid_TagLost);
                    _rfid.open();

                    //Wait for a Phidget RFID to be attached before doing anything with 
                    //the object
                    Console.WriteLine("waiting for attachment...");
                    _rfid.waitForAttachment();

                    //turn on the antenna and the led to show everything is working
                    _rfid.Antenna = true;
                    _rfid.LED = true;
                }
                catch (PhidgetException ex)
                {
                    Console.WriteLine(ex.Description);
                }
            }

            //attach event handler...display the serial number of the attached RFID phidget
            public void rfid_Attach(object sender, AttachEventArgs e)
            {
                Console.WriteLine("RFIDReader {0} attached!", e.Device.SerialNumber.ToString());
            }

            //detach event handler...display the serial number of the detached RFID phidget
            public void rfid_Detach(object sender, DetachEventArgs e)
            {
                _phidgetServer.Terminate();
                Console.WriteLine("RFID reader {0} detached!",
                                        e.Device.SerialNumber.ToString());
            }

            //Print the tag code of the scanned tag
            public void rfid_Tag(object sender, TagEventArgs e)
            {
                PhidgetRFIDModule phidgetData = (PhidgetRFIDModule)_phidgetData;
                phidgetData.Tag = e.Tag;
                phidgetData.Protocol = e.protocol.ToString();
                // on first access thrown an exception.
                try
                {
                    phidgetData.LastTag = _rfid.LastTag;
                    phidgetData.LastTagProtocol = _rfid.LastTagProtocol.ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                try
                {
                    // Call SendToPhidgetClients(PSx, data).
                    Console.WriteLine(string.Format("SendToPhidgetClients: {0} / {1}", _phidgetServerDetails.MachineName, _phidgetData.ToString()));
                    _phidgetServer.PhidgetServerHubProxy.Invoke("SendToPhidgetClients", _phidgetServerDetails, phidgetData).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Console.WriteLine("!!! There was an error on SendToPhidgetClients call: {0}  \n", task.Exception.GetBaseException().Message);
                        }
                    }).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Console.WriteLine("Tag {0} scanned", e.Tag);
            }

            //print the tag code for the tag that was just lost
            public void rfid_TagLost(object sender, TagEventArgs e)
            {
                Console.WriteLine("Tag {0} lost", e.Tag);
            }
            public void rfidClosePhidgetHardware()
            {
                //turn off the led
                _rfid.LED = false;

                //close the phidget and dispose of the object
                _rfid.Attach -= new AttachEventHandler(rfid_Attach);
                _rfid.Detach -= new DetachEventHandler(rfid_Detach);
                _rfid.Tag -= new TagEventHandler(rfid_Tag);
                _rfid.TagLost -= new TagEventHandler(rfid_TagLost);
                _rfid.Error -= new Phidgets.Events.ErrorEventHandler(rfid_Error);

                _rfid.close();
                _rfid = null;
            }
            //Error event handler...display the error description string
            static void rfid_Error(object sender, Phidgets.Events.ErrorEventArgs e)
            {
                Console.WriteLine(e.Description);
            }

            #endregion

            #region PhidgetModuleType.PhidgetServo
            public void servoInitPhidgetHardware()
            {
                try
                {
                    //Declare a Servo object
                    _servo = new Servo();

                    //Hook the basic event handlers
                    _servo.Attach += new AttachEventHandler(servo_Attach);
                    _servo.Detach += new DetachEventHandler(servo_Detach);
                    _servo.Error += new Phidgets.Events.ErrorEventHandler(servo_Error);

                    //hook the phidget specific event handlers
                    _servo.PositionChange += new PositionChangeEventHandler(servo_PositionChange);

                    //open the Servo object for device connections
                    _servo.open();

                    //Get the program to wait for a Servo to be attached
                    Console.WriteLine("Waiting for Servo to be attached...");
                    _servo.waitForAttachment();
                }
                catch (PhidgetException ex)
                {
                    Console.WriteLine(ex.Description);
                }
            }
            public void servoClosePhidgetHardware()
            {
                _servo.Attach -= new AttachEventHandler(servo_Attach);
                _servo.Detach -= new DetachEventHandler(servo_Detach);
                _servo.PositionChange -= new PositionChangeEventHandler(servo_PositionChange);
                _servo.Error -= new Phidgets.Events.ErrorEventHandler(servo_Error);

                //Servo object
                _servo.close();

                //set the object to null to get it out of memory
                _servo = null;

            }
            //Attach event handler...Display te serial number of the attached servo device
            public void servo_Attach(object sender, AttachEventArgs e)
            {
                Console.WriteLine("Servo {0} attached!", e.Device.SerialNumber.ToString());
            }

            //Detach event handler...Display the serial number of the detached servo device
            public void servo_Detach(object sender, DetachEventArgs e)
            {
                Console.WriteLine("Servo {0} detached!", e.Device.SerialNumber.ToString());
            }

            //Error event handler....Display the error description to the console
            public void servo_Error(object sender, Phidgets.Events.ErrorEventArgs e)
            {
                Console.WriteLine(e.Description);
            }

            //Position CHange event handler...display which motor changed position and 
            //its new position value to the console
            public void servo_PositionChange(object sender, PositionChangeEventArgs e)
            {
                Console.WriteLine("Servo {0} Position {1}", e.Index, e.Position);
            }

            #endregion

            #region PhidgetModuleType.PhidgetStarterKit
            public void ifKitInitPhidgetHardware()
            {
                try
                {
                    //Initialize the InterfaceKit object
                    _ifKit = new InterfaceKit();

                    //Hook the basica event handlers
                    _ifKit.Attach += new AttachEventHandler(ifKit_Attach);
                    _ifKit.Detach += new DetachEventHandler(ifKit_Detach);
                    _ifKit.Error += new Phidgets.Events.ErrorEventHandler(ifKit_Error);

                    //Hook the phidget spcific event handlers
                    _ifKit.InputChange += new InputChangeEventHandler(ifKit_InputChange);
                    _ifKit.OutputChange += new OutputChangeEventHandler(ifKit_OutputChange);
                    _ifKit.SensorChange += new SensorChangeEventHandler(ifKit_SensorChange);

                    //Open the object for device connections
                    _ifKit.open();

                    //Wait for an InterfaceKit phidget to be attached
                    Console.WriteLine("Waiting for InterfaceKit to be attached...");
                    _ifKit.waitForAttachment();

                }
                catch (PhidgetException ex)
                {
                    Console.WriteLine(ex.Description);
                }

            }
            //Attach event handler...Display the serial number of the attached InterfaceKit 
            //to the console
            public void ifKit_Attach(object sender, AttachEventArgs e)
            {
                Console.WriteLine("InterfaceKit {0} attached!",
                                    e.Device.SerialNumber.ToString());
            }

            //Detach event handler...Display the serial number of the detached InterfaceKit 
            //to the console
            public void ifKit_Detach(object sender, DetachEventArgs e)
            {
                _phidgetServer.Terminate();
                Console.WriteLine("InterfaceKit {0} detached!",
                                    e.Device.SerialNumber.ToString());
            }

            //Error event handler...Display the error description to the console
            public void ifKit_Error(object sender, Phidgets.Events.ErrorEventArgs e)
            {
                Console.WriteLine(e.Description);
            }

            //Input Change event handler...Display the input index and the new value to the 
            //console
            public void ifKit_InputChange(object sender, InputChangeEventArgs e)
            {
                Console.WriteLine("Input index {0} value {1}", e.Index, e.Value.ToString());
            }

            //Output change event handler...Display the output index and the new valu to 
            //the console
            public void ifKit_OutputChange(object sender, OutputChangeEventArgs e)
            {
                Console.WriteLine("Output index {0} value {1}", e.Index, e.Value.ToString());

                try
                {
                    PhidgetStarterKitModule phidgetData = (PhidgetStarterKitModule)_phidgetData;
                    phidgetData.DigitalOutput[e.Index] = e.Value;
                    // Call SendToPhidgetClients(PSx, data).
                    Console.WriteLine(string.Format("SendToPhidgetClients: {0} / {1}", _phidgetServerDetails.MachineName, _phidgetData.ToString()));
                    _phidgetServer.PhidgetServerHubProxy.Invoke("SendToPhidgetClients", _phidgetServerDetails, phidgetData).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Console.WriteLine("!!! There was an error on SendToPhidgetClients call: {0}  \n", task.Exception.GetBaseException().Message);
                        }
                    }).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //Sensor Change event handler...Display the sensor index and it's new value to 
            //the console
            public void ifKit_SensorChange(object sender, SensorChangeEventArgs e)
            {
                Console.WriteLine("Sensor index {0} value {1}", e.Index, e.Value);

                try
                {
                    PhidgetStarterKitModule phidgetData = (PhidgetStarterKitModule)_phidgetData;
                    phidgetData.AnalogInput[e.Index] = e.Value;

                    // Call SendToPhidgetClients(PSx, data).
                    Console.WriteLine(string.Format("SendToPhidgetClients: {0} / {1}", _phidgetServerDetails.MachineName, _phidgetData.ToString()));
                    _phidgetServer.PhidgetServerHubProxy.Invoke("SendToPhidgetClients", _phidgetServerDetails, phidgetData).ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            Console.WriteLine("!!! There was an error on SendToPhidgetClients call: {0}  \n", task.Exception.GetBaseException().Message);
                        }
                    }).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            public void ifKitClosePhidgetHardware()
            {
                _ifKit.Attach -= new AttachEventHandler(ifKit_Attach);
                _ifKit.Detach -= new DetachEventHandler(ifKit_Detach);
                _ifKit.InputChange -= new InputChangeEventHandler(ifKit_InputChange);
                _ifKit.OutputChange -= new OutputChangeEventHandler(ifKit_OutputChange);
                _ifKit.SensorChange -= new SensorChangeEventHandler(ifKit_SensorChange);
                _ifKit.Error -= new Phidgets.Events.ErrorEventHandler(ifKit_Error);

                //User input was rad so we'll terminate the program, so close the object
                _ifKit.close();

                //set the object to null to get it out of memory
                _ifKit = null;
            }

            #endregion


        }

    }

}
