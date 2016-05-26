using System;

    public class TcpMessage
    {
        public String Subject;
        public Object Data;

        public TcpMessage(string subject, object data)
        {
            Subject = subject;
            Data = data;
        }
    }
