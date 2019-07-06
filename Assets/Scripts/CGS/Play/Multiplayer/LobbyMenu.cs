﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Mirror;

using CGS.Menu;

namespace CGS.Play.Multiplayer
{
    [RequireComponent(typeof(Modal))]
    public class LobbyMenu : SelectionPanel
    {
        public Button joinButton;

        public SortedList<string, string> HostNames { get; private set; } = new SortedList<string, string>();
        public string SelectedHost { get; private set; } = "";

        private bool _wasDown;
        private bool _wasUp;
        private bool _wasPage;

        private Modal _modal;

        void Start()
        {
            _modal = GetComponent<Modal>();
        }

        void Update()
        {
            if (!_modal.IsFocused)
                return;

            if (Input.GetButtonDown(Inputs.Vertical) || Input.GetAxis(Inputs.Vertical) != 0)
            {
                if (Input.GetAxis(Inputs.Vertical) > 0 && !_wasUp)
                    SelectPrevious();
                else if (Input.GetAxis(Inputs.Vertical) < 0 && !_wasDown)
                    SelectNext();
            }

            if ((Input.GetKeyDown(Inputs.BluetoothReturn) || Input.GetButtonDown(Inputs.Submit)) && joinButton.interactable)
                Join();
            else if (Input.GetKeyDown(Inputs.BluetoothReturn) && Toggles.Contains(EventSystem.current.currentSelectedGameObject))
                EventSystem.current.currentSelectedGameObject.GetComponent<Toggle>().isOn = true;
            else if (Input.GetButtonDown(Inputs.New))
                Host();
            else if ((Input.GetButtonDown(Inputs.PageVertical) || Input.GetAxis(Inputs.PageVertical) != 0) && !_wasPage)
                ScrollPage(Input.GetAxis(Inputs.PageVertical));
            else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown(Inputs.Cancel))
                Hide();

            _wasDown = Input.GetAxis(Inputs.Vertical) < 0;
            _wasUp = Input.GetAxis(Inputs.Vertical) > 0;
            _wasPage = Input.GetAxis(Inputs.PageVertical) != 0;
        }

        public void Show(UnityAction cancelAction)
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            HostNames.Clear();
            SelectedHost = string.Empty;
            Rebuild(HostNames, SelectHost, SelectedHost);

            CardGameManager.Instance.discovery.lobby = this;
            CardGameManager.Instance.discovery.SearchForHost();
        }

        public void DisplayHosts(List<string> hosts)
        {
            if (hosts == null || hosts.OrderBy(t => t).ToList().SequenceEqual(HostNames.Keys.ToList()))
                return;

            HostNames.Clear();
            foreach (string host in hosts)
                HostNames[host] = host.Split(':').Last();

            if (!HostNames.ContainsValue(SelectedHost))
            {
                SelectedHost = string.Empty;
                joinButton.interactable = false;
            }
            Rebuild(HostNames, SelectHost, SelectedHost);
        }

        public void Host()
        {
            NetworkManager.singleton.StartHost();
            Hide();
        }

        public void SelectHost(Toggle toggle, string host)
        {
            if (string.IsNullOrEmpty(host))
            {
                SelectedHost = string.Empty;
                joinButton.interactable = false;
                return;
            }

            if (toggle.isOn)
            {
                SelectedHost = host;
                joinButton.interactable = true;
            }
            else if (!toggle.group.AnyTogglesOn() && SelectedHost.Equals(host))
                Join();
        }

        public void Join()
        {
            NetworkManager.singleton.networkAddress = HostNames[SelectedHost];
            NetworkManager.singleton.StartClient();
            Hide();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
