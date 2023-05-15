using LAMA.Models;
using LAMA.Singletons;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.ViewModels
{
    public class ChatChannelsItemViewModel : BaseViewModel
    {
        private string _channelName;
        public string ChannelName
        {
            get { return _channelName; }
            set { SetProperty(ref _channelName, value); }
        }

        private bool _isVisible;
        public bool IsVisible {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        private bool _canArchive;
        public bool CanArchive
        {
            get { return _canArchive; }
            set { SetProperty(ref _canArchive, value); }
        }

        private bool _canRestore;
        public bool CanRestore
        {
            get { return _canRestore; }
            set { SetProperty(ref _canRestore, value); }
        }

        private bool archived;

        public ChatChannelsItemViewModel(string channel)
        {
            archived = channel[0] == Communicator.SpecialCharacters.archivedChannelIndicator;
            if (archived) _channelName = channel.Substring(1);
            else _channelName = channel;
            _canArchive = CommunicationInfo.Instance.IsServer && !archived;
            _canRestore = CommunicationInfo.Instance.IsServer && archived;
            _isVisible = CommunicationInfo.Instance.IsServer || !archived;
        }
    }
}
