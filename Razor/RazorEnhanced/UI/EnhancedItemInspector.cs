﻿using System;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace RazorEnhanced.UI
{
	public partial class EnhancedItemInspector : Form
	{
		private Thread m_ProcessInfo;
		private Assistant.Item m_itemTarg;

		internal EnhancedItemInspector(Assistant.Item itemTarg)
		{
			InitializeComponent();
			MaximizeBox = false;
			m_itemTarg = itemTarg;
		}
		private void razorButton1_Click(object sender, EventArgs e)
		{
			try
			{
				m_ProcessInfo.Abort();
			}
			catch { }

			this.Close();
		}

		private void bNameCopy_Click(object sender, EventArgs e)
		{
			Clipboard.SetText(lName.Text);
		}

		private void bSerialCopy_Click(object sender, EventArgs e)
		{
			Clipboard.SetText(lSerial.Text);
		}

		private void bItemIdCopy_Click(object sender, EventArgs e)
		{
			Clipboard.SetText(lItemID.Text);
		}

		private void bColorCopy_Click(object sender, EventArgs e)
		{
			Clipboard.SetText(lColor.Text);
		}

		private void bPositionCopy_Click(object sender, EventArgs e)
		{
			Clipboard.SetText(lPosition.Text);
		}

		private void bContainerCopy_Click(object sender, EventArgs e)
		{
			Clipboard.SetText(lContainer.Text);
		}

		private void bRContainerCopy_Click(object sender, EventArgs e)
		{
			Clipboard.SetText(lRootContainer.Text);
		}

		private void bAmountCopy_Click(object sender, EventArgs e)
		{
			Clipboard.SetText(lAmount.Text);
		}

		private void bLayerCopy_Click(object sender, EventArgs e)
		{
			Clipboard.SetText(lLayer.Text);
		}

		private void bOwnedCopy_Click(object sender, EventArgs e)
		{
			Clipboard.SetText(lOwned.Text);
		}

		private void EnhancedItemInspector_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				m_ProcessInfo.Abort();
			}
			catch { }
		}

		private void EnhancedItemInspector_Load(object sender, EventArgs e)
		{
			// general
			lSerial.Text = "0x" + m_itemTarg.Serial.Value.ToString("X8");
			lItemID.Text = "0x" + m_itemTarg.ItemID.Value.ToString("X4");
			lColor.Text = "0x" + m_itemTarg.Hue.ToString("X4");
			lPosition.Text = m_itemTarg.Position.ToString();
			// Details
			Assistant.PlayerData tempdata;
			Assistant.Item tempdata2;
			if (m_itemTarg.Container is Assistant.PlayerData)
			{
				tempdata = (Assistant.PlayerData)m_itemTarg.Container;
				lContainer.Text = tempdata.Serial.ToString();
			}
			if (m_itemTarg.Container is Assistant.Item)
			{
				tempdata2 = (Assistant.Item)m_itemTarg.Container;
				lContainer.Text = tempdata2.Serial.ToString();
			}

			if (m_itemTarg.RootContainer is Assistant.PlayerData)
			{
				tempdata = (Assistant.PlayerData)m_itemTarg.RootContainer;
				lRootContainer.Text = tempdata.Serial.ToString();
				if (tempdata.Serial == Assistant.World.Player.Serial)
					lOwned.Text = "Yes";
			}
			if (m_itemTarg.RootContainer is Assistant.Item)
			{
				tempdata2 = (Assistant.Item)m_itemTarg.RootContainer;
				lRootContainer.Text = tempdata2.Serial.ToString();
				if (tempdata2.Serial == Assistant.World.Player.Backpack.Serial)
					lOwned.Text = "Yes";
			}

			lAmount.Text = m_itemTarg.Amount.ToString();
			lLayer.Text = m_itemTarg.Layer.ToString();

			// Flag
			containerflaglabel.Text = (m_itemTarg.IsContainer) ? "Yes" : "No";
			containerflaglabel.ForeColor = (m_itemTarg.IsContainer) ? Color.Green : Color.Red;

			corpseflaglabel.Text = (m_itemTarg.IsCorpse) ? "Yes" : "No";
			corpseflaglabel.ForeColor = (m_itemTarg.IsCorpse) ? Color.Green : Color.Red;

			doorflaglabel.Text = (m_itemTarg.IsDoor) ? "Yes" : "No";
			doorflaglabel.ForeColor = (m_itemTarg.IsDoor) ? Color.Green : Color.Red;

			multiflaglabel.Text = (m_itemTarg.IsMulti) ? "Yes" : "No";
			multiflaglabel.ForeColor = (m_itemTarg.IsMulti) ? Color.Green : Color.Red;

			potionflaglabel.Text = (m_itemTarg.IsPotion) ? "Yes" : "No";
			potionflaglabel.ForeColor = (m_itemTarg.IsPotion) ? Color.Green : Color.Red;

			movableflaglabel.Text = (m_itemTarg.Movable) ? "Yes" : "No";
			movableflaglabel.ForeColor = (m_itemTarg.Movable) ? Color.Green : Color.Red;

			twohandflaglabel.Text = (m_itemTarg.IsTwoHanded) ? "Yes" : "No";
			twohandflaglabel.ForeColor = (m_itemTarg.IsTwoHanded) ? Color.Green : Color.Red;

			groudflaglabel.Text = (m_itemTarg.OnGround) ? "Yes" : "No";
			groudflaglabel.ForeColor = (m_itemTarg.OnGround) ? Color.Green : Color.Red;

			visibleflaglabel.Text = (m_itemTarg.Visible) ? "Yes" : "No";
			visibleflaglabel.ForeColor = (m_itemTarg.Visible) ? Color.Green : Color.Red;

			// Immagine
			Bitmap m_itemimage = new Bitmap(Ultima.Art.GetStatic(m_itemTarg.ItemID));
			{
				if (m_itemTarg.Hue > 0)
				{
					int hue = m_itemTarg.Hue;
					bool onlyHueGrayPixels = (hue & 0x8000) != 0;
					hue = (hue & 0x3FFF) - 1;
					Ultima.Hue m_hue = Ultima.Hues.GetHue(hue);
					m_hue.ApplyTo(m_itemimage, onlyHueGrayPixels);
				}
				imagepanel.BackgroundImage = m_itemimage;
			}
			// Attributes
			m_ProcessInfo = new Thread(ProcessInfoThread);
			m_ProcessInfo.Start();
		}

		private void ProcessInfoThread()
		{
			if (m_itemTarg != null)
			{
				Items.WaitForProps(m_itemTarg.Serial, 1000);

				if (m_itemTarg.ObjPropList.Content.Count > 0)
				{
					for (int i = 0; i < m_itemTarg.ObjPropList.Content.Count; i++)
					{
						Assistant.ObjectPropertyList.OPLEntry ent = m_itemTarg.ObjPropList.Content[i];
						if (i == 0)
							if (ent.ToString() == null)
								lName.Invoke(new Action(() => lName.Text = m_itemTarg.Name.ToString()));
							else
								lName.Invoke(new Action(() => lName.Text = ent.ToString()));
						string content = ent.ToString();
						listBoxAttributes.Invoke(new Action(() => listBoxAttributes.Items.Add(content)));
					}
				}
				else
				{
					lName.Invoke(new Action(() => lName.Text = m_itemTarg.Name.ToString()));
					listBoxAttributes.Invoke(new Action(() => listBoxAttributes.Items.Add("No Props Readed!")));
				}
			}
		}
	}
}