using RDPCOMAPILib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utils.WindowsApi
{
    public class ScreenSharing
    {
        #region 属性
        /// <summary>
        /// ScreenSharingService
        /// </summary>
        public static ScreenSharing Instance = new Lazy<ScreenSharing>(() => new ScreenSharing()).Value;

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectString { get; private set; }

        /// <summary>
        /// 是否正在分享
        /// </summary>
        private bool _isOpen;
        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (value != _isOpen)
                {
                    _isOpen = value;
                    if (_isOpen)
                    {
                        OnRdpShareOpen?.Invoke(nameof(ScreenSharing), null);
                    }
                    else
                    {
                        OnRdpShareClose?.Invoke(nameof(ScreenSharing), null);
                    }
                }
            }
        }

        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool IsPause { get; private set; }

        /// <summary>
        /// 是否加密
        /// </summary>
        public bool IsEncrypt { get; private set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public RDPErrorType ErrorMessage { get; private set; }

        /// <summary>
        /// 远程桌面错误类型
        /// </summary>
        public enum RDPErrorType
        {
            NoNetworkReady = 1,
            UnKnow = 0
        }
        #endregion

        #region 事件
        public event EventHandler<AttendeeConnectInfoEventArgs> OnAttendeeConnected;
        public event EventHandler<AttendeeDisConnectEventArgs> OnAttendeeDisconnected;
        public event EventHandler OnGraphicsStreamPaused;
        public event EventHandler OnGraphicsStreamResumed;
        public event EventHandler OnRdpShareOpen;
        public event EventHandler OnRdpShareClose;
        #endregion

        #region 字段
        private string _authID = "Coder";

        private string _groupName = "ScreenSharing";

        private RDPSession m_pRdpSession = null;

        private CTRL_LEVEL _attendeeControlLevel;

        private List<IRDPSRAPIAttendee> _attendees;

        private IRDPSRAPIInvitation _rDPSRAPIInvitation;

        protected object _lock = new object();
        #endregion

        #region 构造
        private ScreenSharing()
        {
            _attendees = new List<IRDPSRAPIAttendee>();
        }
        #endregion

        #region 事件注册方法
        /// <summary>
        /// 错误
        /// </summary>
        /// <param name="ErrorInfo"></param>
        private void OnError(object ErrorInfo)
        {
            ///待议
        }

        /// <summary>
        /// 远程桌面恢复
        /// </summary>
        private void GraphicsStreamResumed()
        {
            IsPause = false;
            OnGraphicsStreamResumed?.Invoke(this, null);
        }

        /// <summary>
        /// 远程桌面暂停
        /// </summary>
        private void GraphicsStreamPaused()
        {
            IsPause = true;
            OnGraphicsStreamPaused?.Invoke(this, null);
        }

        /// <summary>
        /// 连接着申请权限
        /// </summary>
        /// <param name="pAttendee"></param>
        /// <param name="RequestedLevel"></param>
        private void OnControlLevelChangeRequest(object pAttendee, CTRL_LEVEL RequestedLevel)
        {
            ///待议 -纠结点（1.完全由主机赋予 2.与会者可以申请）
            ///判断设置的与会者权限赋值方式
        }

        /// <summary>
        /// 连接者连入
        /// </summary>
        /// <param name="pAttendee"></param>
        private void AttendeeConnected(object pAttendee)
        {
            IRDPSRAPIAttendee rDPSRAPIAttendee = pAttendee as IRDPSRAPIAttendee;

            rDPSRAPIAttendee.ControlLevel = _attendeeControlLevel;

            _attendees.Add(rDPSRAPIAttendee);

            OnAttendeeConnected?.Invoke(this, new AttendeeConnectInfoEventArgs(rDPSRAPIAttendee.RemoteName, rDPSRAPIAttendee.ConnectivityInfo.PeerIP, rDPSRAPIAttendee.Id, rDPSRAPIAttendee.ControlLevel == CTRL_LEVEL.CTRL_LEVEL_INTERACTIVE));
        }

        /// <summary>
        /// 连接者断开
        /// </summary>
        /// <param name="pDisconnectInfo"></param>
        private void AttendeeDisconnected(object pDisconnectInfo)
        {
            IRDPSRAPIAttendeeDisconnectInfo pDiscInfo = pDisconnectInfo as IRDPSRAPIAttendeeDisconnectInfo;

            RemoveAndDisposeAttendee(pDiscInfo.Attendee);

            while (Marshal.ReleaseComObject(pDiscInfo) > 0) ;
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 注册所需要的事件
        /// </summary>
        private void RegistEvent()
        {
            m_pRdpSession = new RDPSession();

            m_pRdpSession.OnAttendeeConnected += new _IRDPSessionEvents_OnAttendeeConnectedEventHandler(AttendeeConnected);

            m_pRdpSession.OnAttendeeDisconnected += new _IRDPSessionEvents_OnAttendeeDisconnectedEventHandler(AttendeeDisconnected);

            m_pRdpSession.OnControlLevelChangeRequest += new _IRDPSessionEvents_OnControlLevelChangeRequestEventHandler(OnControlLevelChangeRequest);

            m_pRdpSession.OnGraphicsStreamPaused += new _IRDPSessionEvents_OnGraphicsStreamPausedEventHandler(GraphicsStreamPaused);

            m_pRdpSession.OnGraphicsStreamResumed += new _IRDPSessionEvents_OnGraphicsStreamResumedEventHandler(GraphicsStreamResumed);

            m_pRdpSession.OnError += new _IRDPSessionEvents_OnErrorEventHandler(OnError);

            try
            {
                m_pRdpSession.Open();
            }
            catch (Exception ex)
            {
                Close();
                ErrorMessage = RDPErrorType.NoNetworkReady;
            }
        }

        /// <summary>
        /// 判读对象是否为空，为空抛出异常
        /// </summary>
        /// <param name="parameter">检查对象</param>
        /// <param name="errorMsg">异常信息</param>
        private void ObjIsNull(object parameter, string errorMsg)
        {
            if (object.Equals(parameter, null))
            {
                throw new Exception(errorMsg);
            }
        }

        /// <summary>
        /// 为私有列表消除对象并释放对象
        /// </summary>
        /// <param name="attendee"></param>
        private void RemoveAndDisposeAttendee(IRDPSRAPIAttendee attendee)
        {
            for (int i = 0; i < _attendees.Count; i++)
            {
                if (_attendees[i].Id == attendee.Id)
                {
                    OnAttendeeDisconnected?.Invoke(this, new AttendeeDisConnectEventArgs(attendee.Id));

                    int id = _attendees[i].Id;

                    IRDPSRAPIAttendee rDPSRAPI = _attendees[i];

                    while (Marshal.FinalReleaseComObject(rDPSRAPI) > 0) ;

                    _attendees.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 得到对应的私有对象
        /// </summary>
        /// <param name="attendee"></param>
        /// <returns></returns>
        private IRDPSRAPIAttendee GetRealAttendee(int id)
        {
            for (int i = 0; i < _attendees.Count; i++)
            {
                if (_attendees[i].Id == id)
                {
                    return _attendees[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 将连接字符串以xml形式保存
        /// </summary>
        /// <param name="InviteString"></param>
        private void WriteToFile(string InviteString)
        {
            string exePath = Environment.CurrentDirectory;
            using (StreamWriter sw = File.CreateText(exePath + "\\inv.xml"))
            {
                sw.WriteLine(InviteString);
            }
        }

        /// <summary>
        /// 删除链接字符串文件
        /// </summary>
        private void DeleteConnectFile()
        {
            string exePath = Environment.CurrentDirectory;
            if (File.Exists(exePath + "\\inv.xml"))
            {
                File.Delete(exePath + "\\inv.xml");
            }
        }

        /// <summary>
        /// 是否是Windows7
        /// </summary>
        /// <returns></returns>
        private bool IsWindows7()
        {
            Version currentVersion = Environment.OSVersion.Version;

            if (currentVersion >= new Version(6, 1) && currentVersion < new Version(6, 2))
            {
                return true;
            }

            return false;
        }
        #endregion

        #region 公有方法
        /// <summary>
        /// 创建会话
        /// </summary>
        /// <param name="password">会话连接密码</param>
        /// <param name="attendeeLimit">会话最多加入人数</param>
        /// <returns>会话连接字符串</returns>
        public void CreateInvitation(string password, int attendeeLimit, bool isControl)
        {
            lock (_lock)
            {
                ///判空
                if (!object.Equals(m_pRdpSession, null))
                    return;

                RegistEvent();

                if (m_pRdpSession != null)
                {
                    if (isControl)
                    {
                        _attendeeControlLevel = CTRL_LEVEL.CTRL_LEVEL_INTERACTIVE;
                    }
                    else
                    {
                        _attendeeControlLevel = CTRL_LEVEL.CTRL_LEVEL_VIEW;
                    }

                    _rDPSRAPIInvitation = m_pRdpSession.Invitations.CreateInvitation(_authID, _groupName, password, attendeeLimit);

                    ConnectString = _rDPSRAPIInvitation.ConnectionString;

                    //WriteToFile(LinkString);

                    IsPause = false;

                    IsEncrypt = password != string.Empty;

                    IsOpen = true;
                }
            }
        }

        /// <summary>
        /// 暂停会话
        /// </summary>
        public void Pause()
        {
            if (object.Equals(m_pRdpSession, null))
                return;

            m_pRdpSession.Pause();
        }

        /// <summary>
        /// 恢复会话
        /// </summary>
        public void Resume()
        {
            if (object.Equals(m_pRdpSession, null))
                return;

            m_pRdpSession.Resume();
        }

        /// <summary>
        /// 关闭会话(PC)
        /// </summary>
        public void Close()
        {
            lock (_lock)
            {
                if (object.Equals(m_pRdpSession, null))
                    return;

                foreach (var p in _attendees)
                {
                    p.TerminateConnection();
                }

                if (_attendees.Count > 0)
                {
                    foreach (var p in _attendees)
                    {
                        while (Marshal.FinalReleaseComObject(p) > 0) ;
                    }
                }

                _attendees.Clear();

                m_pRdpSession.Close();

                if (IsWindows7())
                {
                    while (Marshal.FinalReleaseComObject(_rDPSRAPIInvitation) > 0) ;

                    while (Marshal.FinalReleaseComObject(m_pRdpSession) > 0) ;
                }

                m_pRdpSession = null;

                GC.Collect();

                //DeleteConnectFile();

                IsOpen = false;
            }
        }

        /// <summary>
        /// 获取连接者列表
        /// </summary>
        /// <returns></returns>
        public List<AttendeeInfo> GetAttendeeInfos()
        {
            List<AttendeeInfo> attendeeInfos = new List<AttendeeInfo>();

            for (int i = 0; i < _attendees.Count; i++)
            {
                attendeeInfos.Add(new AttendeeInfo()
                {
                    ID = _attendees[i].Id,
                    RemoteName = _attendees[i].RemoteName,
                    PeerIP = _attendees[i].ConnectivityInfo.PeerIP,
                    IsControl = _attendees[i].ControlLevel == CTRL_LEVEL.CTRL_LEVEL_INTERACTIVE
                });
            }

            return attendeeInfos;
        }

        /// <summary>
        /// 单独修改某个连接的控制权限
        /// </summary>
        /// <param name="attendee">与会者对象</param>
        /// <param name="controlLevel">想要赋予的权限</param>
        public void SetInterActiveControlLevel(int id)
        {
            ///判空
            if (object.Equals(m_pRdpSession, null))
                return;

            IRDPSRAPIAttendee realAttendee = GetRealAttendee(id);

            if (object.Equals(realAttendee, null))
                return;

            realAttendee.ControlLevel = CTRL_LEVEL.CTRL_LEVEL_INTERACTIVE;
        }

        public void SetViewControlLevel(int id)
        {
            ///判空
            if (object.Equals(m_pRdpSession, null))
                return;

            IRDPSRAPIAttendee realAttendee = GetRealAttendee(id);

            if (object.Equals(realAttendee, null))
                return;

            realAttendee.ControlLevel = CTRL_LEVEL.CTRL_LEVEL_VIEW;
        }

        /// <summary>
        /// 关闭某个指定的与会者连接
        /// </summary>
        /// <param name="attendee"></param>
        public void CloseAttendeeConnect(int id)
        {
            ///判空
            if (object.Equals(m_pRdpSession, null))
                return;

            IRDPSRAPIAttendee realAttendee = GetRealAttendee(id);

            if (object.Equals(realAttendee, null))
                return;

            realAttendee.TerminateConnection();
        }
        #endregion
    }
}
