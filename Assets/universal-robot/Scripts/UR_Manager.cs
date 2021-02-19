using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Xml;

public class UR_Manager
{
    TCPClientBuilder remoteClient;
    TCPClientBuilder remoteClientRealTime;
    URFrame m_frame = new URFrame();
    URFrameRealTime m_frame_RT = new URFrameRealTime();
    bool m_frame_RT_readed = false;
    string m_ip;

    #region Connection


    /// <summary>
    /// Starts the connection with the robot
    /// </summary>
    /// <param name="ip">IP adress of the robot</param>
    public void Connect(string ip)
    {
        m_ip = ip;
        remoteClientRealTime = new TCPClientBuilder(m_ip, 30003, 1000, 1000, NetworkBuilder.NETWORK_MODE.READ_WRITE, ParseRealTime);
        remoteClient = new TCPClientBuilder(m_ip, 30002, 1000, 1000, NetworkBuilder.NETWORK_MODE.READ_WRITE, Parse);
        //Debug.Log( "Connecting to UR at " + m_ip );
    }


    /// <summary>
    /// Checks if connection is established
    /// </summary>
    /// <returns>true if robot is connected</returns>
    public bool isConnected()
    {
        return remoteClientRealTime.connected && m_frame_RT_readed;
    }


    /// <summary>
    /// Disconnect sockets to UR
    /// </summary>
    public void Disconnect()
    {
        Debug.Log("UR disconnecting at " + m_ip);

        if (remoteClientRealTime != null)
        {
            remoteClientRealTime.Disconnect();
        }

        if (remoteClient != null)
        {
            remoteClient.Disconnect();
        }
    }


    #endregion

    #region Frame Structures

    public struct URFrame
    {
        public RobotModeData robotMode;
        public JointData[] joints;
        public CartesianData cartesian;
        public MasterBoardData masterboard;
        public ToolData tool;
    }

    //! \struct Cartesian data definition
    public struct CartesianData
    {
        public double x;
        //!< Tool vector, X-value
        public double y;
        //!< Tool vector, Y-value
        public double z;
        //!< Tool vector, Z-value
        public double rx;
        //!< Rx: Axis angle of the tool orientation
        public double ry;
        //!< Ry: Axis angle of the tool orientation
        public double rz;
        //!< Rz: Axis angle of the tool orientation
    }

    //! Robot mode data definitions
    public struct ToolData
    {
        public char analogInputRange2;
        public char analogInputRange3;
        public double analogInput2;
        public double analogInput3;
        public float toolVoltage48V;
        public byte toolOutputVoltage;
        public float toolCurrent;
        public float toolTemperature;
        public ToolMode toolMode;
    }

    public enum ToolMode
    {
        TOOL_BOOTLOADER_MODE = 249,
        TOOL_RUNNING_MODE = 253,
        TOOL_IDLE_MODE = 255
    }

    //! \enum Joint modes definitions
    public enum JointMode
    {
        JOINT_SHUTTING_DOWN_MODE = 236,
        JOINT_PART_D_CALIBRATION_MODE = 237,
        JOINT_BACKDRIVE_MODE = 238,
        JOINT_POWER_OFF_MODE = 239,
        JOINT_EMERGENCY_STOPPED_MODE = 240,
        JOINT_CALVAL_INITIALIZATION_MODE = 241,
        JOINT_ERROR_MODE = 242,
        JOINT_FREEDRIVE_MODE = 243,
        JOINT_SIMULATED_MODE = 244,
        JOINT_NOT_RESPONDING_MODE = 245,
        JOINT_MOTOR_INITIALISATION_MODE = 246,
        JOINT_ADC_CALIBRATION_MODE = 247,
        JOINT_DEAD_COMMUTATION_MODE = 248,
        JOINT_BOOTLOADER_MODE = 249,
        JOINT_CALIBRATION_MODE = 250,
        JOINT_STOPPED_MODE = 251,
        JOINT_FAULT_MODE = 252,
        JOINT_RUNNING_MODE = 253,
        JOINT_INITIALIZATION_MODE = 254,
        JOINT_IDLE_MOD = 255
    }

    //! \struct Joint data definition
    public struct JointData
    {
        public double q_actual;
        //!< Each axis actual positions
        public double q_target;
        //!< Each axis target positions
        public double qd_actual;
        //!< Each axis actual speeds
        public float I_actual;
        //!< Each axis actual currents
        public float V_actual;
        //!< Each axis actual volts
        public float T_motor;
        //!< Each axis motor's temperature
        public float T_micro;
        //!< Deprecated
        public JointMode jointmode;
        //!< Each axis joint mode
    }

    //! \enum Robot modes definitions
    public enum RobotMode
    {
        ROBOT_MODE_DISCONNECTED = 0,
        ROBOT_MODE_CONFIRM_SAFETY = 1,
        ROBOT_MODE_BOOTING = 2,
        ROBOT_MODE_POWER_OFF = 3,
        ROBOT_MODE_POWER_ON = 4,
        ROBOT_MODE_IDLE = 5,
        ROBOT_MODE_BACKDRIVE = 6,
        ROBOT_MODE_RUNNING = 7,
    }

    public enum ControlRobotMode
    {
        CONTROL_MODE_POSITION = 0,
        CONTROL_MODE_TEACH = 1,
        CONTROL_MODE_FORCE = 2,
        CONTROL_MODE_TORQUE = 3
    }
    //! Robot mode data definitions
    public struct RobotModeData
    {
        public ulong timeStamp;
        public bool robotConnected;
        public bool realRobotEnabled;
        public bool powerOnRobot;
        public bool emergencyStopped;
        public bool securityStopped;
        public bool programRunning;
        public bool programPaused;
        public RobotMode robotMode;
        public double speedFraction;
        public ControlRobotMode controlMode;
        public double speedScaling;
    }

    //! Robot mode data definitions
    public struct MasterBoardData
    {
        public int digitalInputBits;
        public int digitalOutputBits;


        public char analogInputRange0;
        public char analogInputRange1;
        public double analogInput0;
        public double analogInput1;
        public char analogOutputDomain0;
        public char analogOutputDomain1;
        public double analogOutput0;
        public double analogOutput1;
        public float masterboardTemperature;
        public float robotVoltage48V;
        public float robotCurrent;
        public float masterIOCurrent;
        public MasterSafetyState masterSafetyState;
        public MasterOnOffState masterOnOffState;
        public char euromap67Installed;
        //		public int euromapInputBits;
        //		public int euromapOutputBits;
        //		public short euromapVoltage;
        //		public short euromapCurrent;

        //CB3
        public SafetyMode safetyMode;
        public InReducedMode inreducedmode;

    }

    public enum InReducedMode
    {
        UNKNOWN_VALUE = 0
    }

    public enum SafetyMode
    {
        SAFETY_MODE_NORMAL = 1,
        SAFETY_MODE_REDUCED = 2,
        SAFETY_MODE_PROTECTIVE_STOP = 3,
        SAFETY_MODE_RECOVERY = 4,
        SAFETY_MODE_SAFEGUARD_STOP = 5,
        SAFETY_MODE_SYSTEM_EMERGENCY_STOP = 6,
        SAFETY_MODE_ROBOT_EMERGENCY_STOP = 7,
        SAFETY_MODE_VIOLATION = 8,
        SAFETY_MODE_FAULT = 9
    }

    public enum MasterSafetyState
    {
        SAFETY_STATE_UNDEFINED = 0,
        SAFETY_STATE_BOOTLOADER = 1,
        SAFETY_STATE_FAULT = 2,
        SAFETY_STATE_BOOTING = 3,
        SAFETY_STATE_INITIALIZING = 4,
        SAFETY_STATE_ROBOT_EMERGENCY_STOP = 5,
        SAFETY_STATE_EXTERNAL_EMERGENCY_STOP = 6,
        SAFETY_STATE_SAFEGUARD_STOP = 7,
        SAFETY_STATE_OK = 8
    }

    public enum MasterOnOffState
    {
        ROBOT_48V_STATE_OFF = 0,
        ROBOT_48V_STATE_TURNING_ON = 1,
        ROBOT_48V_STATE_ON = 2,
        ROBOT_48V_TURNING_OFF = 3
    }

    #endregion

    #region RealTime Frame Structures

    //! Robot information realTIme
    public struct URFrameRealTime
    {
        public double time;
        //!< time elapsed since controler started
        public double[] q_target;
        //!< position to reach for each axis
        public double[] qd_target;
        //!< speed to reach for each axis
        public double[] qdd_target;
        //!< acceleration to reach for each axis
        public double[] I_target;
        //!< current to reach for each axis
        public double[] M_target;
        //!< torque to reach for each axis
        public double[] q_actual;
        //!< actual position for each axis
        public double[] qd_actual;
        //!< actual speed for each axis
        public double[] I_actual;
        //!< actual current for each axis
        public double[,] accel;
        //!< obsolete (x,y,z acceleration for each axis)
        public double[] tcp_force;
        //!< TCP force for each axis
        public double[] tool_vector;
        //!< tool position cartesian coordinates
        public double[] tcp_speed;
        //!< tool speed cartesian coordinates
        public Int64 digital_inputs;
        //!< state of digital inputs
        public double[] motor_temp;
        //!< temperature in celcius degrees for each axis
        public double controller_time;
        //!< execution time of real-time thread controler
        public double test_value;
        //!< used only by Universal RObot software
        //Software 1.8+

        public RobotMode robotMode;
        //!< Robot control mode
        public JointMode[] jointModes;
        //!< Joint control mode



        //CB3

        public double[] I_control;
        //!< joint control current
        public double[] tool_vector_target;
        //!< Target Cartesian coordinates of the tool: (x,y,z,rx,ry,rz), where rx, ry and rz is a rotation vector representation of the tool orientation
        public double[] tcp_speed_target;
        //!< Target speed of the tool given in Cartesian coordinates
        public SafetyMode safety_mode;
        //!< Safety mode
        public double[] tool_acceleration_values;
        //!< Tool x,y and z accelerometer values (software version 1.7)
        public double speed_scaling;
        //!< Speed scaling of the trajectory limiter
        public double linear_momentum_norm;
        //!< Norm of Cartesian linear momentum
        public double V_main;
        //!< Masterboard: Main voltage
        public double V_robot;
        //!< Masterboard: Robot voltage (48V)
        public double I_robot;
        //!< Masterboard: Robot current
        public double[] V_actual;
        public Int64 digital_outputs;
        public double program_state;
    }
    #endregion

    #region Parse 30002 socket

    void Parse(byte[] cmd, byte[] message)
    {
        try
        {
            if (message.Length < 10)
                return;

            BinaryReaderBigEndian reader = new BinaryReaderBigEndian(new MemoryStream(message));
            int size = reader.ReadInt32();

            if (size != message.Length)
                return;

            byte type = reader.ReadByte();
            if (type != 16)
                return;

            URFrame f = new URFrame();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                int subPacketSize = reader.ReadInt32();
                type = reader.ReadByte();

                switch (type)
                {
                    case 0:
                        f.robotMode = ParseRobotMode(reader.ReadBytes(subPacketSize - 5));
                        break;
                    case 1:
                        f.joints = ParseJointData(reader.ReadBytes(subPacketSize - 5));
                        break;
                    case 2:
                        f.tool = ParseToolData(reader.ReadBytes(subPacketSize - 5));
                        break;
                    case 3:
                        f.masterboard = ParseMasterBoardData(reader.ReadBytes(subPacketSize - 5));
                        break;

                    case 4:
                        f.cartesian = ParseCartesianData(reader.ReadBytes(subPacketSize - 5));
                        break;
                    /*						
				case 5:
					reader.ReadBytes(subPacketSize-5);
					break;				
				case 6:
					reader.ReadBytes(subPacketSize-5);
					break;
					*/

                    default:
                        //Debug.LogWarning ("unknown subpacket type "+type+" length:"+subPacketSize);
                        // swallow unhandled/unknown packets bytes
                        reader.ReadBytes(subPacketSize - 5);
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    //! Robot mode data parsing
    RobotModeData ParseRobotMode(byte[] raw)
    {
        BinaryReaderBigEndian reader = new BinaryReaderBigEndian(new MemoryStream(raw));
        RobotModeData r = new RobotModeData();

        r.timeStamp = reader.ReadUInt64();
        r.robotConnected = reader.ReadByte() > 0;
        r.realRobotEnabled = reader.ReadByte() > 0;
        r.powerOnRobot = reader.ReadByte() > 0;
        r.emergencyStopped = reader.ReadByte() > 0;
        r.securityStopped = reader.ReadByte() > 0;
        r.programRunning = reader.ReadByte() > 0;
        r.programPaused = reader.ReadByte() > 0;

        r.robotMode = (RobotMode)(reader.ReadByte());
        r.controlMode = (ControlRobotMode)reader.ReadByte();
        r.speedFraction = reader.ReadDouble();
        r.speedScaling = reader.ReadDouble();

        return r;
    }

    //! Joint data parsing
    JointData[] ParseJointData(byte[] raw)
    {
        BinaryReaderBigEndian reader = new BinaryReaderBigEndian(new MemoryStream(raw));
        JointData[] r = new JointData[6];

        // Note: despite what the Sysaxes documentation states, each joint frame is sent separately
        for (int i = 0; i < 6; i++)
        {
            r[i].q_actual = reader.ReadDouble();
            r[i].q_target = reader.ReadDouble();
            r[i].qd_actual = reader.ReadDouble();
            r[i].I_actual = reader.ReadFloat();
            r[i].V_actual = reader.ReadFloat();
            r[i].T_motor = reader.ReadFloat();
            r[i].T_micro = reader.ReadFloat();
            r[i].jointmode = (JointMode)reader.ReadByte();
        }

        return r;
    }

    ToolData ParseToolData(byte[] raw)
    {
        BinaryReaderBigEndian reader = new BinaryReaderBigEndian(new MemoryStream(raw));


        ToolData t;

        t.analogInputRange2 = reader.ReadChar();
        t.analogInputRange3 = reader.ReadChar();
        t.analogInput2 = reader.ReadDouble();
        t.analogInput3 = reader.ReadDouble();
        t.toolVoltage48V = reader.ReadFloat();
        t.toolOutputVoltage = reader.ReadByte();
        t.toolCurrent = reader.ReadFloat();
        t.toolTemperature = reader.ReadFloat();
        t.toolMode = (ToolMode)reader.ReadByte();
        return t;

    }

    MasterBoardData ParseMasterBoardData(byte[] raw)
    {
        MasterBoardData r = new MasterBoardData();
        BinaryReaderBigEndian reader = new BinaryReaderBigEndian(new MemoryStream(raw));

        r.digitalInputBits = reader.ReadInt16();
        r.digitalOutputBits = reader.ReadInt16();

        r.digitalInputBits = reader.ReadInt32();
        r.digitalOutputBits = reader.ReadInt32();

        r.analogInputRange0 = reader.ReadChar();
        r.analogInputRange1 = reader.ReadChar();
        r.analogInput0 = reader.ReadDouble();
        r.analogInput1 = reader.ReadDouble();
        r.analogOutputDomain0 = reader.ReadChar();
        r.analogOutputDomain1 = reader.ReadChar();
        r.analogOutput0 = reader.ReadDouble();
        r.analogOutput1 = reader.ReadDouble();

        r.masterboardTemperature = reader.ReadFloat();
        r.robotVoltage48V = reader.ReadFloat();
        r.robotCurrent = reader.ReadFloat();
        r.masterIOCurrent = reader.ReadFloat();


        r.safetyMode = (SafetyMode)reader.ReadByte();

        r.inreducedmode = InReducedMode.UNKNOWN_VALUE;
        reader.ReadByte();

        // THIS IS WRONG IDK WHY
        r.euromap67Installed = reader.ReadChar();

        //unused
        reader.ReadBytes(4);


        //		r.euromapInputBits			= reader.ReadInt32();
        //		r.euromapOutputBits			= reader.ReadInt32();
        //		r.euromapVoltage			= reader.ReadInt16();
        //		r.euromapCurrent			= reader.ReadInt16();

        return r;
    }

    //! Cartesian data parsing definition
    CartesianData ParseCartesianData(byte[] raw)
    {
        BinaryReaderBigEndian reader = new BinaryReaderBigEndian(new MemoryStream(raw));
        CartesianData r;
        r.x = reader.ReadDouble();
        r.y = reader.ReadDouble();
        r.z = reader.ReadDouble();
        r.rx = reader.ReadDouble();
        r.ry = reader.ReadDouble();
        r.rz = reader.ReadDouble();
        return r;
    }

    #endregion

    #region Parse 30003 socket

    void ParseRealTime(byte[] cmd, byte[] message)
    {
        try
        {
            if (message.Length < 1060)
                return;

            BinaryReaderBigEndian reader = new BinaryReaderBigEndian(new MemoryStream(message));
            uint size = (uint)reader.ReadInt32();

            URFrameRealTime f = ParseFrameRealTime(reader.ReadBytes((int)size - 4));
            m_frame_RT = f;
            m_frame_RT_readed = true;
            //Debug.Log (message.Length);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }



    //! Parse a whole raw frame
    URFrameRealTime ParseFrameRealTime(byte[] raw)
    {
        URFrameRealTime f = new URFrameRealTime();
        //Debug.LogError ("parsing frame "+raw.Length);
        BinaryReaderBigEndian reader = new BinaryReaderBigEndian(new MemoryStream(raw));


        f.time = reader.ReadDouble();

        f.q_target = new double[6];
        f.q_target[0] = reader.ReadDouble();
        f.q_target[1] = reader.ReadDouble();
        f.q_target[2] = reader.ReadDouble();
        f.q_target[3] = reader.ReadDouble();
        f.q_target[4] = reader.ReadDouble();
        f.q_target[5] = reader.ReadDouble();

        f.qd_target = new double[6];
        f.qd_target[0] = reader.ReadDouble();
        f.qd_target[1] = reader.ReadDouble();
        f.qd_target[2] = reader.ReadDouble();
        f.qd_target[3] = reader.ReadDouble();
        f.qd_target[4] = reader.ReadDouble();
        f.qd_target[5] = reader.ReadDouble();

        f.qdd_target = new double[6];
        f.qdd_target[0] = reader.ReadDouble();
        f.qdd_target[1] = reader.ReadDouble();
        f.qdd_target[2] = reader.ReadDouble();
        f.qdd_target[3] = reader.ReadDouble();
        f.qdd_target[4] = reader.ReadDouble();
        f.qdd_target[5] = reader.ReadDouble();

        f.I_target = new double[6];
        f.I_target[0] = reader.ReadDouble();
        f.I_target[1] = reader.ReadDouble();
        f.I_target[2] = reader.ReadDouble();
        f.I_target[3] = reader.ReadDouble();
        f.I_target[4] = reader.ReadDouble();
        f.I_target[5] = reader.ReadDouble();

        f.M_target = new double[6];
        f.M_target[0] = reader.ReadDouble();
        f.M_target[1] = reader.ReadDouble();
        f.M_target[2] = reader.ReadDouble();
        f.M_target[3] = reader.ReadDouble();
        f.M_target[4] = reader.ReadDouble();
        f.M_target[5] = reader.ReadDouble();

        f.q_actual = new double[6];
        f.q_actual[0] = reader.ReadDouble();
        f.q_actual[1] = reader.ReadDouble();
        f.q_actual[2] = reader.ReadDouble();
        f.q_actual[3] = reader.ReadDouble();
        f.q_actual[4] = reader.ReadDouble();
        f.q_actual[5] = reader.ReadDouble();



        f.qd_actual = new double[6];
        f.qd_actual[0] = reader.ReadDouble();
        f.qd_actual[1] = reader.ReadDouble();
        f.qd_actual[2] = reader.ReadDouble();
        f.qd_actual[3] = reader.ReadDouble();
        f.qd_actual[4] = reader.ReadDouble();
        f.qd_actual[5] = reader.ReadDouble();

        f.I_actual = new double[6];
        f.I_actual[0] = reader.ReadDouble();
        f.I_actual[1] = reader.ReadDouble();
        f.I_actual[2] = reader.ReadDouble();
        f.I_actual[3] = reader.ReadDouble();
        f.I_actual[4] = reader.ReadDouble();
        f.I_actual[5] = reader.ReadDouble();

        f.I_control = new double[6];
        f.I_control[0] = reader.ReadDouble();
        f.I_control[1] = reader.ReadDouble();
        f.I_control[2] = reader.ReadDouble();
        f.I_control[3] = reader.ReadDouble();
        f.I_control[4] = reader.ReadDouble();
        f.I_control[5] = reader.ReadDouble();

        f.tool_vector = new double[6];
        f.tool_vector[0] = reader.ReadDouble();
        f.tool_vector[1] = reader.ReadDouble();
        f.tool_vector[2] = reader.ReadDouble();
        f.tool_vector[3] = reader.ReadDouble();
        f.tool_vector[4] = reader.ReadDouble();
        f.tool_vector[5] = reader.ReadDouble();



        f.tcp_speed = new double[6];
        f.tcp_speed[0] = reader.ReadDouble();
        f.tcp_speed[1] = reader.ReadDouble();
        f.tcp_speed[2] = reader.ReadDouble();
        f.tcp_speed[3] = reader.ReadDouble();
        f.tcp_speed[4] = reader.ReadDouble();
        f.tcp_speed[5] = reader.ReadDouble();

        f.tcp_force = new double[6];
        f.tcp_force[0] = reader.ReadDouble();
        f.tcp_force[1] = reader.ReadDouble();
        f.tcp_force[2] = reader.ReadDouble();
        f.tcp_force[3] = reader.ReadDouble();
        f.tcp_force[4] = reader.ReadDouble();
        f.tcp_force[5] = reader.ReadDouble();

        f.tool_vector_target = new double[6];
        f.tool_vector_target[0] = reader.ReadDouble();
        f.tool_vector_target[1] = reader.ReadDouble();
        f.tool_vector_target[2] = reader.ReadDouble();
        f.tool_vector_target[3] = reader.ReadDouble();
        f.tool_vector_target[4] = reader.ReadDouble();
        f.tool_vector_target[5] = reader.ReadDouble();

        f.tcp_speed_target = new double[6];
        f.tcp_speed_target[0] = reader.ReadDouble();
        f.tcp_speed_target[1] = reader.ReadDouble();
        f.tcp_speed_target[2] = reader.ReadDouble();
        f.tcp_speed_target[3] = reader.ReadDouble();
        f.tcp_speed_target[4] = reader.ReadDouble();
        f.tcp_speed_target[5] = reader.ReadDouble();

        f.digital_inputs = (Int64)reader.ReadDouble();
        //			byte[] bytearray = reader.ReadBytes(8);
        //			f.digital_inputs = BitConverter.ToInt64(bytearray,0);


        f.motor_temp = new double[6];
        f.motor_temp[0] = reader.ReadDouble();
        f.motor_temp[1] = reader.ReadDouble();
        f.motor_temp[2] = reader.ReadDouble();
        f.motor_temp[3] = reader.ReadDouble();
        f.motor_temp[4] = reader.ReadDouble();
        f.motor_temp[5] = reader.ReadDouble();

        f.controller_time = reader.ReadDouble();

        f.test_value = reader.ReadDouble();

        f.robotMode = (RobotMode)reader.ReadDouble();

        f.jointModes = new JointMode[6];
        f.jointModes[0] = (JointMode)reader.ReadDouble();
        f.jointModes[1] = (JointMode)reader.ReadDouble();
        f.jointModes[2] = (JointMode)reader.ReadDouble();
        f.jointModes[3] = (JointMode)reader.ReadDouble();
        f.jointModes[4] = (JointMode)reader.ReadDouble();
        f.jointModes[5] = (JointMode)reader.ReadDouble();

        f.safety_mode = (SafetyMode)reader.ReadDouble();

        //unused
        reader.ReadBytes(48);

        f.tool_acceleration_values = new double[3];
        f.tool_acceleration_values[0] = reader.ReadDouble();
        f.tool_acceleration_values[1] = reader.ReadDouble();
        f.tool_acceleration_values[2] = reader.ReadDouble();

        //unused
        reader.ReadBytes(48);

        f.speed_scaling = reader.ReadDouble();

        f.linear_momentum_norm = reader.ReadDouble();

        //unused
        reader.ReadBytes(16);

        f.V_main = reader.ReadDouble();

        f.V_robot = reader.ReadDouble();

        f.I_robot = reader.ReadDouble();

        f.V_actual = new double[6];
        f.V_actual[0] = reader.ReadDouble();
        f.V_actual[1] = reader.ReadDouble();
        f.V_actual[2] = reader.ReadDouble();
        f.V_actual[3] = reader.ReadDouble();
        f.V_actual[4] = reader.ReadDouble();
        f.V_actual[5] = reader.ReadDouble();

        if (raw.Length > 1055)
        {
            f.digital_outputs = (Int64)reader.ReadDouble();
            f.program_state = reader.ReadDouble();
        }
        return f;
    }

    #endregion

    #region Send programs

    /// <summary>
    /// Send a program to 30003 socket
    /// </summary>
    /// <param name="program">urscript program</param>
    public void SendProgram(string program)
    {
        //Debug.Log( program );
        if (isConnected())
            remoteClientRealTime.PendCommand(program);
    }

    /// <summary>
    /// Send a program to the dashboard server (port 29999)
    /// </summary>
    /// <param name="message">dashboard command</param>
    void SendDashboardMessage(string message)
    {
        if (remoteClientRealTime.connected)
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(IPAddress.Parse(m_ip), 29999);

                StreamWriter swDB = new StreamWriter(client.GetStream());
                swDB.Write(message);
                Debug.Log("Dashboard : " + message);
                swDB.Close();
                client.Close();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
    #endregion

    #region Get values from robot
    /// <summary>
    /// Gets the actual cartesian coordinates of the tcp
    /// </summary>
    /// <returns> (x,y,z,rx,ry,rz) where rx,ry, rz is a rotation vector representation of the orientation</returns>
    public double[] GetTCPPosition()
    {
        return m_frame_RT.tool_vector;
    }

    /// <summary>
    /// Gets the actual speed of the of the tcp in cartesian coordinates
    /// </summary>
    /// <returns> (xd,yd,zd,rxd,ryd,rzd) where rxd,ryd, rzd is a rotation vector representation of the orientation</returns>
    public double[] GetTCPSpeed()
    {
        return m_frame_RT.tcp_speed;
    }

    /// <summary>
    /// Gets the generalised forces in the tcp
    /// </summary>
    /// <returns> (Fx,Fy,Fz,Mx,My,Mz) </returns>
    public double[] GetTCPForce()
    {
        return m_frame_RT.tcp_force;
    }

    /// <summary>
    /// Gets the digital inputs
    /// </summary>
    /// <returns>Int64 where each bit is a digtal input</returns>
    public Int64 GetInputBits()
    {
        return m_frame_RT.digital_inputs;
    }

    /// <summary>
	/// Gets the digital outputs
	/// </summary>
	/// <returns>Int64 where each bit is a digtal output</returns>
	public Int64 GetOutputBits()
    {
        return m_frame_RT.digital_outputs;
    }


    /// <summary>
    /// Gets the analog inputs
    /// </summary>
    /// <returns>Array of 4 analog inputs</returns>
    public double[] GetAnalogInputs()
    {
        double[] analog = new double[4];
        analog[0] = m_frame.masterboard.analogInput0;
        analog[1] = m_frame.masterboard.analogInput1;
        analog[2] = m_frame.tool.analogInput2;
        analog[3] = m_frame.tool.analogInput3;

        return analog;
    }

    /// <summary>
    /// Gets the joint positions
    /// </summary>
    /// <returns>Array of six angular values in radians</returns>
    public double[] GetJointPositions()
    {
        return m_frame_RT.q_actual;
    }

    /// <summary>
    /// Gets the joint speeds
    /// </summary>
    /// <returns>Array of six angular speed values in radians per second</returns>
    public double[] GetJointSpeeds()
    {
        return m_frame_RT.qd_actual;
    }
    #endregion

    #region Command programs
    /// <summary>
    /// enum of IO output voltage available
    /// </summary>
    public enum V_OUTPUT
    {
        V_0,
        V_12,
        V_24
    }

    /// <summary>
    /// Sets the IO output voltage of the robot
    /// </summary>
    /// <param name="v">output voltage to set</param>
    public void SetVOutput(V_OUTPUT v)
    {
        string prog = "";
        prog += "def setVOutput():\n";
        switch (v)
        {
            case V_OUTPUT.V_0:
                prog += " set_tool_voltage(0)\n";
                break;
            case V_OUTPUT.V_12:
                prog += " set_tool_voltage(12)\n";
                break;
            case V_OUTPUT.V_24:
                prog += " set_tool_voltage(24)\n";
                break;
        }
        prog += "end\n";
        SendProgram(prog);
    }

    /// <summary>
    /// Sets the tcp position in the coordinates system of the robot
    /// </summary>
    /// <param name="tcp_vector">translation vector in meters from the toolmount</param>
    /// <param name="rotation_Vector">rotation vector in radians</param>
    public void SetTCPPosition(Vector3 tcp_vector, Vector3 rotation_Vector)
    {
        string prog = "";
        prog += "def SetTCPPosition():\n";
        prog += string.Format(" set_tcp(p[{0},{1},{2},{3},{4},{5}])\n", tcp_vector.x, tcp_vector.y, tcp_vector.z, rotation_Vector.x, rotation_Vector.y, rotation_Vector.z);
        prog += "end\n";
        SendProgram(prog);
    }

    /// <summary>
    /// Sets the payload mass and center of gravity
    /// </summary>
    /// <param name="mass">mass in kilograms</param>
    /// <param name="cog">center of gravity in meters from the toolmount</param>
    public void SetPayload(double mass, Vector3 cog)
    {
        string prog = "";
        prog += "def SetPayload():\n";
        prog += string.Format(" set_payload({0},[{1},{2},{3}])\n", mass, cog.x, cog.y, cog.z);
        prog += "end\n";
        SendProgram(prog);
    }


    /// <summary>
    /// Stops the robot in linear way with an acceleration of 1 m/s²
    /// </summary>
    public void StopL()
    {
        string prog = "stopl(1)\n";
        SendProgram(prog);
    }

    public void ETHDMover(float s, float d, float m)
    {
        string prog = "";
        string speed = (s).ToString();
        string init = (d).ToString();
        string end = (m).ToString();
        prog += "movel(p[-.777029096313, " + init + ", .095492224143, 1.402371810343, -.366439029978, -2.306243565556], t=" + speed + ")\n";
        prog += "movel(p[-.777029096313, " + end + ", .095492224143, 1.402371810343, -.366439029978, -2.306243565556], t=" + speed + ")\n";
        SendProgram(prog);
    }

    public void ETHDMasterMover(float s, float d, float m)
    {
        string prog = "";
        string speed = (s).ToString();
        string init = (d).ToString();
        string end = (m).ToString();
        prog += "movel(p[-.777029096313, " + init + ", .095492224143, 1.402371810343, -.366439029978, -2.306243565556], a=0.3, v=" + speed + ")\n";
        prog += "movel(p[-.777029096313, " + end + ", .095492224143, 1.402371810343, -.366439029978, -2.306243565556], a=0.3, v=" + speed + ")\n";
        SendProgram(prog);
    }

    public void ETHDMasterMover2DX(float s, float ix, float iy, float fx, float fy)
    {
        string prog = "";
        string speed = (s).ToString();
        string initX = (ix).ToString();
        string initY = (iy).ToString();
        string finX = (fx).ToString();
        string finY = (fy).ToString();
        prog += "movel(p[" + initY  + " , " + initX + ", .135492224143, 1.402371810343, -.366439029978, -2.306243565556], a=0.3, v=" + speed + ")\n";
        prog += "movel(p[" + finY + ", " + finX + ", .135492224143, 1.402371810343, -.366439029978, -2.306243565556], a=0.3, v=" + speed + ")\n";
        SendProgram(prog);
    }

    public void ETHDMasterMover2D(float s, float ix, float iy, float fx, float fy)
    {
        string prog = "";
        string speed = (s).ToString();
        string initX = (ix).ToString();
        string initY = (iy).ToString();
        string finX = (fx).ToString();
        string finY = (fy).ToString();
        prog += "movel(p[" + initY + " , " + initX + ", .16869, 0.0495, -0.0888, 4.7334], a=0.3, v=" + speed + ")\n";
        prog += "movel(p[" + finY + ", " + finX + ", .16869,  0.0495, -0.0888, 4.7334], a=0.3, v=" + speed + ")\n";
        SendProgram(prog);
    }

    public void ETHDMasterMoverTime2D(float s, float ix, float iy, float fx, float fy)
    {
        string prog = "";
        string speed = (s).ToString();
        string initX = (ix).ToString();
        string initY = (iy).ToString();
        string finX = (fx).ToString();
        string finY = (fy).ToString();
        prog += "movel(p[" + initY + " , " + initX + ", .16869, 0.0495, -0.0888, 4.7334], t=" + speed + ")\n";
        prog += "movel(p[" + finY + ", " + finX + ", .16869,  0.0495, -0.0888, 4.7334], t=" + speed + ")\n";
        SendProgram(prog);
    }

    /// <summary>
    /// Moves the tcp to a position in the coordinates system of the robot
    /// </summary>
    /// <param name="poseT">translation vector in meters</param>
    /// <param name="poseR">rotation vector in radians</param>
    /// <param name="a">acceleration in m/s²</param>
    /// <param name="v">speed in m/s</param>
    /// <param name="t">time to reach in s</param>
    /// <param name="r">bend radius in meters</param>
    public void MoveL( Vector3 poseT, Vector3 poseR, double a = 1.2, double v = 0.25, double t = 0, double r = 0 )
	{
		string prog = "";
		prog += "def MoveL():\n";
		prog += string.Format( " movel(p[{0},{1},{2},{3},{4},{5}],{6},{7},{8},{9})\n", poseT.x, poseT.y, poseT.z, poseR.x, poseR.y, poseR.z, a, v, t, r );
		prog += "end\n";
		SendProgram( prog );
	}

	/// <summary>
	/// Moves the tcp to a position in the coordinates system of the robot
	/// </summary>
	/// <param name="poseT">translation vector in meters</param>
	/// <param name="poseR">rotation vector in radians</param>
	/// <param name="a">acceleration in rad/s²</param>
	/// <param name="v">speed in rad/s</param>
	/// <param name="t">time to reach in s</param>
	/// <param name="r">bend radius in meters</param>
	public void MoveJ(Vector3 poseT, Vector3 poseR, double a = 0.7, double v = 0.55, double t = 0, double r = 0)
	{
		string prog = "";
		prog += "def MoveJ():\n";
		prog += string.Format(" movej(p[{0},{1},{2},{3},{4},{5}],{6},{7},{8},{9})\n", poseT.x, poseT.y, poseT.z, poseR.x, poseR.y, poseR.z, a, v, t, r);
		prog += "end\n";
		SendProgram(prog);
	}

	public void MoveJ(double[] q, double a = 0.7, double v = 0.55, double t = 0, double r = 0)
	{
		string prog = "";
		prog += "def MoveJ():\n";
		prog += string.Format(" movej([{0},{1},{2},{3},{4},{5}],{6},{7},{8},{9})\n", q[0], q[1], q[2], q[3], q[4], q[5], a, v, t, r);
		prog += "end\n";
		SendProgram(prog);
	}

	/// <summary>
	/// Accelerate linearly in Cartesian space and continue with constant tool speed
	/// </summary>
	/// <param name="tSpeed">spatial vector</param>
	/// <param name="rSpeed">rotation vector</param>
	/// <param name="acceleration">tool linear acceleration</param>
	/// <param name="minTime">time staying in the function</param>
	/// <param name="rotAcc">tool angular acceleration</param>
	public void SpeedL( Vector3 tSpeed, Vector3 rSpeed, float acceleration = 0.5f, float minTime = 0.1f, float rotAcc = 0.3f )
	{
		string prog = "";
		prog += "def SpeedL():\n";
		prog += string.Format( " speedl([{0},{1},{2},{3},{4},{5}],{6},{7},{8})\n", tSpeed.x, tSpeed.y, tSpeed.z, rSpeed.x, rSpeed.y, rSpeed.z, acceleration, minTime, rotAcc );
		prog += "end\n";
		SendProgram( prog );
	}
	#endregion

	#region Coordinate system transformation
	/// <summary>
	/// Transform a vector from robot to unity coordinate system
	/// </summary>
	/// <param name="poseT">vector from robot</param>
	/// <returns></returns>
	public Vector3 PoseTToVector3(Vector3 poseT)
	{
		Vector3 v = new Vector3(poseT.x, poseT.z, poseT.y);
		return v;
	}

	/// <summary>
	/// Transform a robot rotation vector from robot to unity quaternion coordinate system
	/// </summary>
	/// <param name="poseR">rotation vector</param>
	/// <returns></returns>
	public Quaternion PoseRToQuaternion( Vector3 poseR )
	{
		poseR = new Vector3( poseR.x, poseR.z, poseR.y );
		Quaternion q = Quaternion.AngleAxis(poseR.magnitude * 180f / Mathf.PI,-poseR.normalized);
		return q;
	}

	/// <summary>
	/// Transform a unity vector to robot coordinate system
	/// </summary>
	/// <param name="v"></param>
	/// <returns></returns>
	public Vector3 Vector3ToPoseT( Vector3 v )
	{
		Vector3 poseT = new Vector3( v.x, v.z, v.y );
		return poseT; 
	}

	/// <summary>
	/// Transform a quaternion  from unity to robot rotation vector
	/// </summary>
	/// <param name="q">unity quaternion</param>
	/// <returns></returns>
	public Vector3 QuaternionToPoseR(Quaternion q )
	{
		Vector3 poseR = Vector3.zero;
		float angle = 0;
		q.ToAngleAxis( out angle, out poseR );
		poseR = -angle * Mathf.PI / 180f * new Vector3( poseR.x, poseR.z, poseR.y );
		return poseR;
	}
	#endregion
}
