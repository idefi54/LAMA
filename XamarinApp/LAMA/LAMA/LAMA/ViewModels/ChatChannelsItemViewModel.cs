﻿using LAMA.Models;
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

        private bool _canRename;
        public bool CanRename
        {
            get { return _canRename; }
            set { SetProperty(ref _canRename, value); }
        }

        public bool archived;

        public ChatChannelsItemViewModel(string channel)
        {
            archived = channel[0] == Communicator.SpecialCharacters.archivedChannelIndicator;
            if (archived) _channelName = channel.Substring(1);
            else _channelName = channel;
            CanArchive = LocalStorage.cp.permissions.Contains(CP.PermissionType.ManageChat) && !archived;
            CanRestore = LocalStorage.cp.permissions.Contains(CP.PermissionType.ManageChat) && archived;
            IsVisible = CanBeVisible();
            CanRename = LocalStorage.cp.permissions.Contains(CP.PermissionType.ManageChat);

            SQLEvents.dataChanged += SQLEvents_dataChanged;
        }

        private void SQLEvents_dataChanged(Serializable changed, int changedAttributeIndex)
        {
            if (changed is CP cp && cp.ID == LocalStorage.cpID && changedAttributeIndex == 9)
            {
                CanArchive = LocalStorage.cp.permissions.Contains(CP.PermissionType.ManageChat) && !archived;
                CanRestore = LocalStorage.cp.permissions.Contains(CP.PermissionType.ManageChat) && archived;
                IsVisible = CanBeVisible();
                CanRename = LocalStorage.cp.permissions.Contains(CP.PermissionType.ManageChat);
            }
        }

        public bool CanBeVisible()
        {
            return LocalStorage.cp.permissions.Contains(CP.PermissionType.ManageChat) || !archived;
        }
    }
}
