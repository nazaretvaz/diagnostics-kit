﻿/**
 *  Part of the Diagnostics Kit
 *
 *  Copyright (C) 2016  Sebastian Solnica
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 */

using LowLevelDesign.Diagnostics.Bishop.Config;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace LowLevelDesign.Diagnostics.Bishop.UI
{
    public partial class TamperingRuleForm : Form
    {
        private readonly bool isNewRule;
        private readonly Func<string, bool> isRuleNameUsed;

        public TamperingRuleForm(Func<string, bool> isRuleNameUsed)
        {
            isNewRule = true;
            this.isRuleNameUsed = isRuleNameUsed;

            InitializeComponent();
        }

        public TamperingRuleForm(RequestTransformation transformation)
        {
            isNewRule = false;
            isRuleNameUsed = null;

            InitializeComponent();

            txtRuleName.Enabled = false;
            txtRuleName.Text = transformation.Name;
            txtHostRegex.Text = transformation.RegexToMatchAgainstHost;
            txtPathAndQueryRegex.Text = transformation.RegexToMatchAgainstPathAndQuery;
            txtDestinationHost.Text = transformation.DestinationHostHeader;
            txtDestinationPathAndQuery.Text = transformation.DestinationPathAndQuery;
            txtDestinationIPs.Text = string.Join(", ", transformation.DestinationIpAddresses);
            txtDestinationPorts.Text = string.Join(", ", transformation.DestinationPorts.Select(p => p.ToString()));
        }

        private void lnkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/lowleveldesign/diagnostics-kit/wiki/5.1.bishop");
        }

        public RequestTransformation GetRequestTransformation()
        {
            return new RequestTransformation {
                Name = RuleName,
                RegexToMatchAgainstHost = RegexToMatchAgainstHost,
                RegexToMatchAgainstPathAndQuery = RegexToMatchAgainstPathAndQuery,
                DestinationHostHeader = DestinationHost,
                DestinationPathAndQuery = DestinationPathAndQuery,
                DestinationIpAddresses = DestinationIPs,
                DestinationPorts = DestinationPorts
            };
        }

        public string RuleName { get { return txtRuleName.Text.Trim(); } }

        public string RegexToMatchAgainstHost { get { return txtHostRegex.Text.Trim(); } }

        public string RegexToMatchAgainstPathAndQuery { get { return txtPathAndQueryRegex.Text.Trim(); } }

        public string DestinationHost { get { return txtDestinationHost.Text.Trim(); } }

        public string DestinationPathAndQuery { get { return txtDestinationPathAndQuery.Text.Trim(); } }

        public string[] DestinationIPs {
            get
            {
                return txtDestinationIPs.Text.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public ushort[] DestinationPorts
        {
            get
            {
                var v = txtDestinationPorts.Text.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var ports = new List<ushort>(v.Length);
                for (int i = 0; i < v.Length; i++)
                {
                    ushort p;
                    if (ushort.TryParse(v[i], out p))
                    {
                        ports.Add(p);
                    }
                }
                return ports.ToArray();
            }
        }

        private bool IsValid()
        {
            return !(string.IsNullOrEmpty(RuleName) ||
                (string.IsNullOrEmpty(RegexToMatchAgainstHost) && string.IsNullOrEmpty(RegexToMatchAgainstPathAndQuery)) ||
                (string.IsNullOrEmpty(DestinationHost) && string.IsNullOrEmpty(DestinationPathAndQuery) &&
                    DestinationIPs.Length == 0 && DestinationPorts.Length == 0));
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (isNewRule && isRuleNameUsed(RuleName))
            {
                MessageBox.Show(this, "The rule name is already in use.", "Invalid data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (IsValid()) {
                DialogResult = DialogResult.OK;
            } else {
                MessageBox.Show(this, "You must provide the rule name, either host regex or path query regex and any of the destinations.",
                    "Invalid data", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
