using System;
using System.Linq;
using System.Windows.Forms;

namespace A3TeleportEditor
{
    public partial class AddTeleportItem : Form
    {
        private A3TeleportEditor _parentForm;

        public AddTeleportItem(A3TeleportEditor parent)
        {
            InitializeComponent();
            this._parentForm = parent;
        }

        private void AddTeleportItem_Load(object sender, EventArgs e)
        {
            this.npcList.DisplayMember = "NpcName";
            this.npcList.ValueMember = "NpcId";
            this.npcList.DataSource = this._parentForm.npcList.Values.ToList();

            this.mapList.DisplayMember = "MapName";
            this.mapList.ValueMember = "MapId";
            this.mapList.DataSource = this._parentForm.mapList.Values.ToList();
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.teleportIndex.Text.Trim()))
            {
                _ = MessageBox.Show("Please enter teleport index", "A3 Teleport Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!uint.TryParse(this.teleportIndex.Text.Trim(), out _))
            {
                _ = MessageBox.Show("Teleport index has to be a positive number", "A3 Teleport Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var teleportIndex = Convert.ToUInt32(this.teleportIndex.Text.Trim());
            if (teleportIndex > this._parentForm.teleportData.Length - 1)
            {
                _ = MessageBox.Show("Teleport index has to be lesser than " + this._parentForm.teleportData.Length, "A3 Teleport Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this._parentForm.teleportItemList.Add(new TeleportItem()
            {
                NpcId = Convert.ToUInt32(this.npcList.SelectedValue),
                MapId = Convert.ToUInt32(this.mapList.SelectedValue),
                TeleportIndex = teleportIndex,
                NpcName = this._parentForm.getNpcName(Convert.ToUInt32(this.npcList.SelectedValue)),
                MapName = this._parentForm.getMapName(Convert.ToUInt32(this.mapList.SelectedValue)),
                ServerMapName = this._parentForm.getServerMapName(teleportIndex)
            });

            this.Close();
        }
    }
}
