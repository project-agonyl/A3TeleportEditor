using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace A3TeleportEditor
{
    public partial class A3TeleportEditor : Form
    {
        public BindingList<TeleportItem> teleportItemList;
        public string[] teleportData;
        public Dictionary<uint, Map> mapList;
        public Dictionary<uint, Npc> npcList;

        public A3TeleportEditor()
        {
            this.InitializeComponent();
        }

        public string getNpcName(uint id)
        {
            if (npcList.ContainsKey(id))
            {
                return npcList[id].NpcName;
            }

            return string.Empty;
        }

        public string getMapName(uint id)
        {
            if (mapList.ContainsKey(id))
            {
                return mapList[id].MapName;
            }

            return string.Empty;
        }

        public string getServerMapName(uint index)
        {
            if (index < teleportData.Length)
            {
                return this.getMapName(Convert.ToUInt32(Regex.Split(teleportData[index], @"\s+")[0].Trim()));
            }

            return string.Empty;
        }

        private void A3TeleportEditor_Load(object sender, EventArgs e)
        {
            if (!File.Exists(this.GetMyDirectory() + Path.DirectorySeparatorChar + "NPC.bin"))
            {
                _ = MessageBox.Show("Please place NPC.bin in same folder as this application", "A3 Teleport Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            if (!File.Exists(this.GetMyDirectory() + Path.DirectorySeparatorChar + "MC.bin"))
            {
                _ = MessageBox.Show("Please place MC.bin in same folder as this application", "A3 Teleport Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            if (!File.Exists(this.GetMyDirectory() + Path.DirectorySeparatorChar + "Teleport.txt"))
            {
                _ = MessageBox.Show("Please place Teleport.txt in same folder as this application", "A3 Teleport Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            this.teleportItemList = new BindingList<TeleportItem>();
            this.dataGridView.AutoGenerateColumns = false;
            this.dataGridView.DataSource = this.teleportItemList;

            this.dataGridView.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "NpcId",
                Name = "NPC ID",
                Width = 50
            });
            this.dataGridView.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "NpcName",
                Name = "NPC Name",
                Width = 150
            });
            this.dataGridView.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "TeleportIndex",
                Name = "Teleport Index",
                Width = 100
            });
            this.dataGridView.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "ServerMapName",
                Name = "Server Map Name",
                Width = 150
            });
            this.dataGridView.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "MapId",
                Name = "Map ID",
                Width = 50
            });
            this.dataGridView.Columns.Add(new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "MapName",
                Name = "Map Name",
                Width = 150
            });

            var mapDataFile = File.ReadAllBytes(this.GetMyDirectory() + Path.DirectorySeparatorChar + "MC.bin");
            this.mapList = new Dictionary<uint, Map>();
            for (var i = 4; i < mapDataFile.Length; i += 56)
            {
                if (this.mapList.ContainsKey(BitConverter.ToUInt32(mapDataFile.Skip(i).Take(4).ToArray(), 0)))
                {
                    continue;
                }

                var item = new Map()
                {
                    MapId = BitConverter.ToUInt32(mapDataFile.Skip(i).Take(4).ToArray(), 0),
                    MapName = System.Text.Encoding.Default.GetString(mapDataFile.Skip(i + 24).Take(32).ToArray())
                };

                this.mapList.Add(item.MapId, item);
            }

            var npcDataFile = File.ReadAllBytes(this.GetMyDirectory() + Path.DirectorySeparatorChar + "NPC.bin");
            this.npcList = new Dictionary<uint, Npc>();
            for (var i = 4; i < npcDataFile.Length; i += 44)
            {
                if (this.npcList.ContainsKey(BitConverter.ToUInt32(npcDataFile.Skip(i).Take(4).ToArray(), 0)))
                {
                    continue;
                }

                var item = new Npc()
                {
                    NpcId = BitConverter.ToUInt32(npcDataFile.Skip(i).Take(4).ToArray(), 0),
                    NpcName = System.Text.Encoding.Default.GetString(npcDataFile.Skip(i + 4).Take(32).ToArray())
                };

                this.npcList.Add(item.NpcId, item);
            }

            this.teleportData = File.ReadAllLines(this.GetMyDirectory() + Path.DirectorySeparatorChar + "Teleport.txt");
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            this.populateGrid();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (this.teleportItemList.Count == 0)
            {
                _ = MessageBox.Show("Teleport list is empty", "A3 Teleport Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (this.saveFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                var dataBuilder = new BinaryDataBuilder();
                dataBuilder.PutBytes(BitConverter.GetBytes(teleportItemList.Count));

                foreach (var item in teleportItemList)
                {
                    dataBuilder.PutBytes(BitConverter.GetBytes(item.NpcId));
                    dataBuilder.PutBytes(BitConverter.GetBytes(item.TeleportIndex));
                    dataBuilder.PutBytes(BitConverter.GetBytes(item.MapId));
                }

                File.WriteAllBytes(saveFileDialog.FileName, dataBuilder.GetBuffer());
                _ = MessageBox.Show("Saved the file " + Path.GetFileName(saveFileDialog.FileName), "A3 Teleport Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            var addTeleportItemForm = new AddTeleportItem(this);
            addTeleportItemForm.ShowDialog();
        }

        private string GetMyDirectory()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }

        private void populateGrid()
        {
            if (this.openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var wlDataFile = File.ReadAllBytes(openFileDialog.FileName);
                this.teleportItemList.Clear();

                for (var i = 4; i < wlDataFile.Length; i += 12)
                {
                    this.teleportItemList.Add(new TeleportItem()
                    {
                        NpcId = BitConverter.ToUInt32(wlDataFile.Skip(i).Take(4).ToArray(), 0),
                        MapId = BitConverter.ToUInt32(wlDataFile.Skip(i + 8).Take(4).ToArray(), 0),
                        TeleportIndex = BitConverter.ToUInt32(wlDataFile.Skip(i + 4).Take(4).ToArray(), 0),
                        NpcName = this.getNpcName(BitConverter.ToUInt32(wlDataFile.Skip(i).Take(4).ToArray(), 0)),
                        MapName = this.getMapName(BitConverter.ToUInt32(wlDataFile.Skip(i + 8).Take(4).ToArray(), 0)),
                        ServerMapName = this.getServerMapName(BitConverter.ToUInt32(wlDataFile.Skip(i + 4).Take(4).ToArray(), 0))
                    });
                }

                _ = MessageBox.Show("Loaded " + teleportItemList.Count + " teleport list items", "A3 Teleport Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
