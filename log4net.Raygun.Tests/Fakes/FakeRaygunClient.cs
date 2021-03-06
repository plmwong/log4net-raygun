﻿using log4net.Raygun.Core;
using Mindscape.Raygun4Net.Messages;

namespace log4net.Raygun.Tests.Fakes
{
    public class FakeRaygunClient : IRaygunClient
    {
        private RaygunMessage _lastMessageSent;

        public RaygunMessage LastMessageSent
        {
            get { return _lastMessageSent; }
        }

        public void Send(RaygunMessage raygunMessage)
        {
            _lastMessageSent = raygunMessage;
        }
    }
}