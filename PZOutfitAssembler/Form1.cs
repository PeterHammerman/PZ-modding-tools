using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Diagnostics;

namespace PZOutfitAssembler
{
    public partial class Form1 : Form
    {
        public string guid;
        public string itemID;
        // string directoryPath = AppDomain.CurrentDomain.BaseDirectory + "/GeneratedFiles/media/scripts/clothing/";
        public string directoryPath = "C:/PZTest/GeneratedFiles/media/scripts/clothing/";
        public string PZinstallPath = "";
        public string GUIDsDir = "/media/";
        public string clothingScriptsDir = "/media/scripts/clothing/";
        public string clothingItemXMLDir = "/media/clothing/clothingItems/";




        public Form1()
        {
            string installPath = GetInstallLocation();
            if (string.IsNullOrEmpty(installPath))
            {
                MessageBox.Show($"Projezt Zomboid is not installed, cannot access registry path", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            InitializeComponent();
            PopulateItemListBoxWithFileNames(directoryPath, listBoxItems);
            if (!string.IsNullOrEmpty(installPath))
            {
                string vanillaItems = installPath + clothingScriptsDir;
                PZinstallPath = installPath;
                PopulateItemListBoxWithFileNames(vanillaItems, listBoxVanila);
            }
        }

        private Dictionary<string, string> ReadItemProperties(string filePath, string selectedItem)
        {
            try
            {
                // Read the entire content of the file
                string fileContent = File.ReadAllText(filePath);

                // Normalize the file content to handle potential whitespace inconsistencies
                fileContent = Regex.Replace(fileContent, @"\s+", " ").Trim();

                // Adjusted pattern to match the specific item block
                string itemPattern = $@"item\s+{Regex.Escape(selectedItem)}\s*\{{(.*?)\}}";
                Match itemMatch = Regex.Match(fileContent, itemPattern, RegexOptions.Singleline);

                if (itemMatch.Success)
                {
                    // Extract the properties block
                    string propertiesBlock = itemMatch.Groups[1].Value;

                    // Adjusted pattern to handle properties with commas
                    string propertyPattern = @"(\w+)\s*=\s*([^,]+),";

                    MatchCollection propertyMatches = Regex.Matches(propertiesBlock, propertyPattern);

                    // Store properties in a dictionary
                    Dictionary<string, string> properties = new Dictionary<string, string>();

                    foreach (Match propertyMatch in propertyMatches)
                    {
                        if (propertyMatch.Groups.Count > 2)
                        {
                            string key = propertyMatch.Groups[1].Value.Trim();
                            string value = propertyMatch.Groups[2].Value.Trim();
                            properties[key] = value;
                        }
                    }

                    return properties; // Return the dictionary of properties
                }
                else
                {
                    MessageBox.Show($"Item '{selectedItem}' not found in the file.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }


        private void ResetItemInfo()
        {


            guid = "";


            textBoxID.Text = "";
            textBoxDisplayName.Text = "";
            //           string displayCategory = checkBoxCanHaveHoles.Checked ? textBoxDisplayName.Text : null;
            textBoxCategory.Text = "";
            textBoxType.Text = "";
            textBoxWeigth.Text = "";
            textBoxTextureIcon.Text = "";
            textBoxBodyLocation.Text = "";
            textBoxBloodLocation.Text = "";
            textBoxChanceToFall.Text = "";
            textBoxClothingItem.Text = ""; 
            textBoxClothingItemExtra.Text = "";
            textBoxClothingItemExtraOption.Text = "";
            textBoxInsulation.Text = "";
            textBoxWind.Text = "";
            textBoxFabric.Text = "";
            textBoxStaticModel.Text = "";
            textBoxTags.Text = "";

            textBoxMaleModel.Text = "";
            textBoxFemaleModel.Text = "";
            textBoxTextureChoices.Text = "";

            textBoxAttachReplacement.Text = "";
            textBoxCapacity.Text = "";
            textBoxCanbeequipped.Text = "";
            textBoxOpenSound.Text = "";
            textBoxCloseSound.Text = "";
            textBoxPutinsound.Text = "";
            textBoxReplaceprimaryhand.Text = "";
            textBoxReplacesecondaryhand.Text = "";
            textBoxRunspeedmodifier.Text = "";
            textBoxWeigthreduction.Text = "";
            textBoxAttachmentProvided.Text = "";

            textBoxIcon.Text = "";
            textBoxBulletDefense.Text = "";
            textBoxScratchDefense.Text = "";
            textBoxDiscomfort.Text = "";
            textBoxCombatSpeed.Text = "";
            textBoxWaterResistance.Text = "";
            textBoxVisionModifier.Text = "";
            textBoxHearing.Text = "";
            textBoxCorpseSicknessDefence.Text = "";
            textBoxDamageSound.Text = "";
            textBoxMaxItemSize.Text = "";
            textBoxSoundParam.Text = "";
            textBoxMetalValue.Text = "";
            textBoxAcceptItemFunction.Text = "";



            checkBoxStatic.Checked = false;
            checkBoxCanHaveHoles.Checked = false;
            checkBoxChanceToFall.Checked = false;
            checkBoxClothingItemExtra.Checked = false;
            checkBoxClothingItemExtraOption.Checked = false;
            checkBoxInsulation.Checked = false;
            checkBoxWind.Checked = false;
            checkBoxFabric.Checked = false;
            checkBoxStaticModel.Checked = false;

            checkBoxCapacity.Checked = false;
            checkBoxCanbeequipped.Checked = false;
            checkBoxOpensound.Checked = false;
            checkBoxClosesound.Checked = false;
            checkBoxPutinsound.Checked = false;
            checkBoxReplaceprimaryhand.Checked = false;
            checkBoxReplacesecondhand.Checked = false;
            checkBoxRunspeedmodifier.Checked = false;
            checkBoxWeigthreduction.Checked = false;
            checkBoxAttachmentProvided.Checked = false;
            checkBoxAttachmentReplacement.Checked = false;

            checkBoxIcon.Checked = false;
            checkBoxBulletDefense.Checked = false;
            checkBoxScratchDefense.Checked = false;
            checkBoxDiscomfort.Checked = false;
            checkBoxCombatSpeed.Checked = false;
            checkBoxWaterResistance.Checked = false;
            checkBoxVisionModifier.Checked = false;
            checkBoxHearingModifier.Checked = false;
            checkBoxCorpseSicknessDefence.Checked = false;
            checkBoxDamageSound.Checked = false;
            checkBoxMaxItemSize.Checked = false;
            checkBoxSoundParam.Checked = false;
            checkBoxMetalValue.Checked = false;
            checkBoxAcceptItemFunction.Checked = false;


            textBoxGUID.Text = guid;

        }


        private string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }

        private void RefreshItemListBox(string directoryPath, ListBox listBox)
        {
            PopulateItemListBoxWithFileNames(directoryPath, listBox);
        }

        private void PopulateItemListBoxWithFileNames(string directoryPath, ListBox listBox)
        {
            try
            {
                // Check if the directory exists
                if (Directory.Exists(directoryPath))
                {
                    // Get all .txt files in the directory
                    string[] files = Directory.GetFiles(directoryPath, "*.txt");

                    // Clear the current items in the ListBox
                    listBox.Items.Clear();

                    string itemPattern = @"item\s+(\w+)";

                    foreach (string file in files)
                    {
                        // Read the content of each file
                        string fileContent = File.ReadAllText(file);

                        // Use regex to find all item names
                        MatchCollection matches = Regex.Matches(fileContent, itemPattern);

                        foreach (Match match in matches)
                        {
                            if (match.Groups.Count > 1)
                            {
                                // Add the item name (captured group) to the ListBox
                                listBox.Items.Add(match.Groups[1].Value);
                            }
                        }
                    }
                }
                else
                {

                    //   MessageBox.Show($"The directory '{directoryPath}' does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while populating the list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e) //Save item
        {

            // Get the file path from textBoxPath
            if (textBoxGUID.TextLength > 0)
            {

                itemID = textBoxID.Text;
                // guid = GenerateGuid();
                string outputPath;

                if (checkBoxDebug.Checked)
                    outputPath = textBoxPath.Text;
                else
                    outputPath = AppDomain.CurrentDomain.BaseDirectory;
                // Validate the output path


                try
                {

                    outputPath += "/GeneratedFiles";
                    // Create the output directory if it doesn't exist
                    Directory.CreateDirectory(outputPath);
                    Directory.CreateDirectory(outputPath + clothingScriptsDir);
                    Directory.CreateDirectory(outputPath + clothingItemXMLDir);
                    Directory.CreateDirectory(outputPath + "/media/clothing/");


                    // Assemble the content for itemname.txt
                    string itemNameContent = AssembleItemNameScript();

                    // Assemble the XML content
                    string xmlContent = AssembleItemXML();
                    string xmlnewOutfit = AssembleOutfitXML();



                    // Save the itemname.txt file
                    string itemNameFilePath = Path.Combine(outputPath + clothingScriptsDir, itemID + ".txt");
                    File.WriteAllText(itemNameFilePath, itemNameContent);

                    string xmlClothItemFilePath = Path.Combine(outputPath + clothingItemXMLDir, textBoxClothingItem.Text + ".xml");
                    File.WriteAllText(xmlClothItemFilePath, xmlContent);



                    string xmlOutfitTableFilePath = Path.Combine(outputPath + "/media/clothing/", "newClothing.xml");
                    File.WriteAllText(xmlOutfitTableFilePath, xmlnewOutfit);

                    MessageBox.Show("item has been generated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    PopulateItemListBoxWithFileNames(directoryPath, listBoxItems);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            else
            {
                MessageBox.Show($"No valid GUID, please generate new before saving", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private string AssembleItemNameScript()
        {
            // Get values from textboxes
            string IDName = textBoxID.Text;
            string displayName = textBoxDisplayName.Text;
            //           string displayCategory = checkBoxCanHaveHoles.Checked ? textBoxDisplayName.Text : null;
            string displayCategory = textBoxCategory.Text;
            string type = textBoxType.Text;
            string weight = textBoxWeigth.Text;
            string iconsForTexture = textBoxTextureIcon.Text;
            string bodyLocation = textBoxBodyLocation.Text;
            string bloodLocation = textBoxBloodLocation.Text;
            string chanceToFall = textBoxChanceToFall.Text;
            string clothingItem = textBoxClothingItem.Text;
            string clothingItemExtra = textBoxClothingItemExtra.Text;
            string clothingItemExtraOption = textBoxClothingItemExtraOption.Text;
            string insulation = textBoxInsulation.Text;
            string wind = textBoxWind.Text;
            string fabrictype = textBoxFabric.Text;
            string tags = textBoxTags.Text;
            string staticModel = textBoxStaticModel.Text;





            string attachreplacement = textBoxAttachReplacement.Text;
            string canbeequipped = textBoxCanbeequipped.Text;
            string capacity = textBoxCapacity.Text;
            string opensound = textBoxOpenSound.Text;
            string closesound = textBoxCloseSound.Text;
            string putinsound = textBoxPutinsound.Text;
            string primaryhand = textBoxReplaceprimaryhand.Text;
            string secondaryhand = textBoxReplacesecondaryhand.Text;
            string runspeedmodifier = textBoxRunspeedmodifier.Text;
            string weigthreduction = textBoxWeigthreduction.Text;
            string attachmentprovided = textBoxAttachmentProvided.Text;

            string icon = textBoxIcon.Text;
            string bulletdef = textBoxBulletDefense.Text;
            string scratchdef = textBoxScratchDefense.Text;
            string discomfort = textBoxDiscomfort.Text;
            string combatspeed = textBoxCombatSpeed.Text;
            string waterres = textBoxWaterResistance.Text;
            string visionmod = textBoxVisionModifier.Text;
            string hearing = textBoxHearing.Text;
            string corpsedef = textBoxCorpseSicknessDefence.Text;

            string damagesound = textBoxDamageSound.Text;
            string maxitemsize = textBoxMaxItemSize.Text;
            string soundparam = textBoxSoundParam.Text;
            string metalvalue = textBoxMetalValue.Text;
            string itemfunction = textBoxAcceptItemFunction.Text;

            string bitedefense = textBoxBiteDefense.Text;


            // Build the script content
            string script = "module Base\n{ \n";
            script += $"    item {IDName}\n";
            script += "    {\n";
            script += $"        DisplayName = {displayName},\n";

            // Add DisplayCategory only if the checkbox is checked
            if (!string.IsNullOrEmpty(displayCategory))
            {
                script += $"        DisplayCategory = {displayCategory},\n";
            }

            script += $"        Type = {type},\n";
            script += $"        Weight = {weight},\n";
            script += $"        IconsForTexture = {iconsForTexture},\n";
            script += $"        BodyLocation = {bodyLocation},\n";
            script += $"        BloodLocation = {bloodLocation},\n";

            if (checkBoxChanceToFall.Checked)
                script += $"        ChanceToFall = {chanceToFall},\n";

            script += $"        ClothingItem = {clothingItem},\n";
            if (checkBoxClothingItemExtra.Checked)
                script += $"        ClothingExtra = {clothingItemExtra},\n";

            if (checkBoxClothingItemExtraOption.Checked)
                script += $"        ClothingExtraOption = {clothingItemExtraOption},\n";

            if (checkBoxInsulation.Checked)
                script += $"        Insulation = {insulation},\n";

            if (checkBoxWind.Checked)
                script += $"        WindResistance = {wind},\n";

            if (checkBoxFabric.Checked)
                script += $"        FabricType = {fabrictype},\n";

            if (checkBoxStaticModel.Checked)
                script += $"        WorldStaticModel = {staticModel},\n";

            if (checkBoxCanHaveHoles.Checked)
                script += $"        CanHaveHoles = TRUE,\n";
            else
                script += $"        CanHaveHoles = FALSE,\n";



            if (checkBoxAttachmentReplacement.Checked)
                script += $"        AttachmentReplacement = {attachreplacement},\n";

            if(checkBoxCanbeequipped.Checked)
                script += $"        CanBeEquipped = {canbeequipped},\n";
            if(checkBoxCapacity.Checked)
                script += $"        Capacity = {capacity},\n";

            if(checkBoxOpensound.Checked)
                script += $"        OpenSound = {opensound},\n";
            if(checkBoxClosesound.Checked)
                script += $"        CloseSound = {closesound},\n";
            if(checkBoxPutinsound.Checked)
                script += $"        PutInSound = {putinsound},\n";
            if(checkBoxReplaceprimaryhand.Checked)
                script += $"        ReplaceInPrimaryHand = {primaryhand},\n";
            if(checkBoxReplacesecondhand.Checked)
                script += $"        ReplaceInSecondHand = {secondaryhand},\n";
            if(checkBoxRunspeedmodifier.Checked)
                script += $"        RunSpeedModifier = {runspeedmodifier},\n";
            if(checkBoxWeigthreduction.Checked)
                script += $"        WeightReduction = {weigthreduction},\n";
            if(checkBoxAttachmentProvided.Checked)
                script += $"        AttachmentProvided = {attachmentprovided},\n";

            if(checkBoxIcon.Checked)
                script += $"        Icon = {icon},\n";
            if (checkBoxBiteDefense.Checked)
                script += $"        BiteDefense = {bitedefense},\n";

            if (checkBoxBulletDefense.Checked)
                script += $"        BulletDefense = {bulletdef},\n";
            if(checkBoxScratchDefense.Checked)
                script += $"        ScratchDefense = {scratchdef},\n";
            if(checkBoxDiscomfort.Checked)
                script += $"        DiscomfortModifier = {discomfort},\n";
            if(checkBoxCombatSpeed.Checked)
                script += $"        CombatSpeedModifier = {combatspeed},\n";
            if(checkBoxWaterResistance.Checked)
                script += $"        WaterRessistance = {waterres},\n";
            if(checkBoxVisionModifier.Checked)
                script += $"        VisionModifier = {visionmod},\n";
            if(checkBoxHearingModifier.Checked)
                script += $"        HearingModifier = {hearing},\n";
            if(checkBoxCorpseSicknessDefence.Checked)
                script += $"        CorpseSicknessDefense = {corpsedef},\n";

            if(checkBoxDamageSound.Checked)
                script += $"        DamageSound = {damagesound},\n";
            if(checkBoxMaxItemSize.Checked)
                script += $"        MaxItemSize = {maxitemsize},\n";
            if(checkBoxSoundParam.Checked)
                script += $"        SoundParameter = {soundparam},\n";
            if(checkBoxMetalValue.Checked)
                script += $"        MetalValue = {metalvalue},\n";

            if(checkBoxAcceptItemFunction.Checked)
                script += $"        AcceptItemFunction = {itemfunction},\n";

            if(checkBoxCosmetic.Checked)
                script += $"        Cosmetic = TRUE,\n";
            if (checkBoxVisualAid.Checked)
                script += $"        VisualAid = TRUE,\n";



            script += $"        Tags = {tags},\n";


            script += "    }\n}";

            return script;
        }

        private string AssembleItemXML()
        {
            // Get values from textboxes
            string maleModel = textBoxMaleModel.Text;
            string femaleModel = textBoxFemaleModel.Text;
            string textureChoices = textBoxTextureChoices.Text;
            bool isStatic = checkBoxStatic.Checked;



            // Build the XML content
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n";
            xml += "<clothingItem>\n";
            xml += $"  <m_MaleModel>{maleModel}</m_MaleModel>\n";
            xml += $"  <m_FemaleModel>{femaleModel}</m_FemaleModel>\n";
            xml += $"  <m_GUID>{guid}</m_GUID>\n";
            xml += $"  <m_Static>{isStatic}</m_Static>\n";
            xml += $"  <m_AllowRandomHue>false</m_AllowRandomHue>\n";
            xml += $"  <m_AllowRandomTint>false</m_AllowRandomTint>\n";
            xml += $"  <m_AttachBone></m_AttachBone>\n";
            xml += $"  <textureChoices>{textureChoices}</textureChoices>\n";
            xml += "</clothingItem>";

            return xml;
        }

        private string AssembleOutfitXML()
        {
            // Get values from textboxes
            string probability = textBoxProbability.Text;
            string clothesName = textBoxClothingName.Text;

            // Build the XML content
            string oxml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n";
            oxml += "<outfitManager>\n";

            if (checkBoxFemale.Checked)
            {
                oxml += $"  <m_FemaleOutfits>\n";
                oxml += $"  <m_Name>{clothesName}</m_Name>\n";
                oxml += $"  <m_Guid>{guid}</m_Guid>\n";

                oxml += $"  <m_Top> false </m_Top>\n";
                oxml += $"  <m_Pants> false </m_Pants>\n";
                oxml += $"  <m_items>\n";
                oxml += $"  <probability>{probability}</probability>\n";
                oxml += $"  <itemGUID>INSERT ITEMS GUI HERE</itemGUID>\n";
                oxml += $"  </m_items>\n";


                oxml += $"  </m_FemaleOutfits>\n";
            }

            if (checkBoxMale.Checked)
            {
                oxml += $"  <m_MaleOutfits>\n";
                oxml += $"  <m_Name>{clothesName}</m_Name>\n";
                oxml += $"  <m_Guid>{guid}</m_Guid>\n";

                oxml += $"  <m_Top> false </m_Top>\n";
                oxml += $"  <m_Pants> false </m_Pants>\n";
                oxml += $"  <m_items>\n";
                oxml += $"  <probability>{probability}</probability>\n";
                oxml += $"  <itemGUID>INSERT ITEMS GUI HERE</itemGUID>\n";
                oxml += $"  </m_items>\n";


                oxml += $"  </m_MaleOutfits>\n";
            }

            oxml += "</outfitManager>";

            return oxml;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            guid = GenerateGuid();
            textBoxGUID.Text = guid;
        }


        private string GetInstallLocation()
        {
            try
            {
                // Define the registry key path
                string registryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 108600";

                // First, attempt to read from the x86 registry view (32-bit)
                using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                                                     .OpenSubKey(registryKeyPath))
                {
                    if (key != null)
                    {
                        object installLocation = key.GetValue("InstallLocation");
                        if (installLocation != null)
                        {
                            return installLocation.ToString(); // Found in 32-bit registry
                        }
                    }
                }

                // If not found in x86, try the x64 registry view
                using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                                                     .OpenSubKey(registryKeyPath))
                {
                    if (key != null)
                    {
                        object installLocation = key.GetValue("InstallLocation");
                        if (installLocation != null)
                        {
                            return installLocation.ToString(); // Found in 64-bit registry
                        }
                    }
                }

                // If neither key exists, show a message
                MessageBox.Show("The InstallLocation value was not found in either the x86 or x64 registry views.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while accessing the registry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void button7_Click(object sender, EventArgs e)  // Edit custom item button
        {
            ResetItemInfo();

            string selectedItem = listBoxItems.SelectedItem.ToString();
            string filePath = @"media/scripts/clothing/" + selectedItem;


            if (checkBoxDebug.Checked)
                filePath = directoryPath + selectedItem + ".txt";
            else
                filePath = AppDomain.CurrentDomain.BaseDirectory + "/GeneratedFiles/media/scripts/clothing/" + selectedItem + ".txt";



            populateItemEditorWindow(filePath, selectedItem);



        }

        private void button9_Click(object sender, EventArgs e) //Clear editor window
        {
            ResetItemInfo();
        }

        private void populateItemEditorWindow(string filePath, string selectedItem)
        {
            Dictionary<string, string> properties = ReadItemProperties(filePath, selectedItem);

            if (properties != null)
            {

                //JSON
                textBoxID.Text = selectedItem;
                var propertyToTextBoxMapping = new Dictionary<string, TextBox>
            {
                { "DisplayName", textBoxDisplayName },
                { "DisplayCategory",textBoxCategory},
                { "Type", textBoxType },
                { "Weight",textBoxWeigth },
                { "IconsForTexture", textBoxTextureIcon },
                { "BodyLocation", textBoxBodyLocation },
                { "BloodLocation", textBoxBloodLocation },
                { "ChanceToFall", textBoxChanceToFall },
                { "ClothingItem", textBoxClothingItem },
                { "ClothingItemExtra", textBoxClothingItemExtra },
                { "ClothingItemExtraOption", textBoxClothingItemExtraOption },
                { "Insulation", textBoxInsulation },
                { "WindResistance", textBoxWind },
                { "FabricType", textBoxFabric },
                { "WorldStaticModel", textBoxStaticModel },
                { "Tags", textBoxTags },

                { "AttachmentReplacement", textBoxAttachReplacement},
                { "CanBeEquipped",textBoxCanbeequipped},
                { "Capacity",textBoxCapacity},
                { "OpenSound",textBoxOpenSound},
                { "CloseSound",textBoxCloseSound},
                { "PutInSound",textBoxPutinsound},
                { "ReplaceInPrimaryHand",textBoxReplaceprimaryhand},
                { "ReplaceInSecondHand", textBoxReplacesecondaryhand},
                { "RunSpeedModifier",textBoxRunspeedmodifier},
                { "WeightReduction",textBoxWeigthreduction},
                { "AttachmentsProvided",textBoxAttachmentProvided},

                            {"Icon", textBoxIcon },
                            {"BiteDefense", textBoxBiteDefense },
                            {"BulletDefense", textBoxBulletDefense },
                            {"ScratchDefense", textBoxScratchDefense },
                            {"DiscomfortModifier", textBoxDiscomfort },
                            {"CombatSpeedModifier", textBoxCombatSpeed },
                            {"WaterRessistance", textBoxWaterResistance },
                            {"VisionModifier", textBoxVisionModifier },
                            {"HearingModifier", textBoxHearing },
                            {"CorpseSicknessDefense", textBoxCorpseSicknessDefence },

                            {"DamageSound", textBoxDamageSound },
                            {"MaxItemSize", textBoxMaxItemSize },
                            {"SoundParameter", textBoxSoundParam },
                            {"MetalValue", textBoxMetalValue },
                            {"AcceptItemFunction", textBoxAcceptItemFunction },

               };

                var propertyToCheckboxMapping = new Dictionary<string, CheckBox>
               {
                { "CanHaveHoles", checkBoxCanHaveHoles },
                { "ChanceToFall", checkBoxChanceToFall },
                { "ClothingItemExtra", checkBoxClothingItemExtra },
                { "ClothingItemExtraOption", checkBoxClothingItemExtraOption },
                { "Insulation", checkBoxInsulation },
                { "WindResistance", checkBoxWind },
                { "FabricType", checkBoxFabric },
                { "WorldStaticModel", checkBoxStaticModel },

                { "CanBeEquipped", checkBoxCanbeequipped},
                { "OpenSound", checkBoxOpensound},
                { "CloseSound", checkBoxClosesound},
                { "PutInSound", checkBoxPutinsound},
                { "ReplaceInPrimaryHand", checkBoxReplaceprimaryhand},
                { "ReplaceInSecondHand", checkBoxReplacesecondhand},
                { "RunSpeedModifier", checkBoxRunspeedmodifier},
                { "WeightReduction", checkBoxWeigthreduction},
                { "AttachmentsProvided",checkBoxAttachmentProvided},

                            {"AttachmentReplacement", checkBoxAttachmentReplacement },

                            {"Icon", checkBoxIcon },
                            {"BiteDefense", checkBoxBiteDefense },
                            {"BulletDefense", checkBoxBulletDefense },
                            {"ScratchDefense", checkBoxScratchDefense },
                            {"DiscomfortModifier", checkBoxDiscomfort },
                            {"CombatSpeedModifier", checkBoxCombatSpeed },
                            {"WaterResisstance", checkBoxWaterResistance },
                            {"VisionModifier", checkBoxVisionModifier },
                            {"HearingModifier", checkBoxHearingModifier },
                            {"CorpseSicknessDefense", checkBoxCorpseSicknessDefence },
                            {"Capacity", checkBoxCapacity },
                            {"DamageSound", checkBoxDamageSound },
                            {"MaxItemSize", checkBoxMaxItemSize },
                            {"SoundParameter", checkBoxSoundParam },
                            {"MetalValue", checkBoxMetalValue },
                            {"AcceptItemFunction", checkBoxAcceptItemFunction },

                            {"Cosmetic", checkBoxCosmetic },
                            {"VisualAid", checkBoxVisualAid },
                // Add more mappings as necessary
               };


                // Populate textboxes
                foreach (var property in properties)
                {
                    if (propertyToTextBoxMapping.ContainsKey(property.Key))
                    {
                        propertyToTextBoxMapping[property.Key].Text = property.Value;
                    }
                }

                var stringValuesForCheckboxes = new HashSet<string>
            {
                "Enabled", "Active", "True", "On", "Yes", "TRUE"
                // Add any other string values that should trigger checkboxes to be checked
            };

                // Populate checkboxes
                foreach (var kvp in propertyToCheckboxMapping)
                {
                    if (properties.ContainsKey(kvp.Key))
                    {
                        string propertyValue = properties[kvp.Key].Trim().ToLowerInvariant();

                        // Check for boolean values (true/false) and string-based true values
                        if (propertyValue == "true" || propertyValue == "1" || propertyValue == "yes" || propertyValue == "enabled")
                        {
                            kvp.Value.Checked = true;
                        }
                        else if (propertyValue == "false" || propertyValue == "0" || propertyValue == "no" || propertyValue == "disabled")
                        {
                            kvp.Value.Checked = false;
                        }
                        else if (float.TryParse(propertyValue, out float floatValue))
                        {
                            // Property has a valid float value, enable the checkbox
                            kvp.Value.Checked = true;
                        }
                        else if (!string.IsNullOrEmpty(propertyValue))
                        {
                            // If the value is a non-empty string (e.g., "Cotton", "media/models/model.fbx"), enable the checkbox
                            kvp.Value.Checked = true;
                        }
                        else
                        {
                            // Unexpected value, log and uncheck the checkbox
                            Console.WriteLine($"Unexpected value for property '{kvp.Key}': {propertyValue}");
                            kvp.Value.Checked = false;
                        }
                    }
                    else
                    {
                        // Property is missing, set checkbox to false
                        kvp.Value.Checked = false;
                    }
                }






                //           string displayCategory = checkBoxCanHaveHoles.Checked ? textBoxDisplayName.Text : null;


            }

            //XML 

            selectedItem = textBoxClothingItem.Text;


            string xmlFilePath = @"media/scripts/clothingItems/" + selectedItem;


            if (checkBoxDebug.Checked)
                xmlFilePath = "C:/PZTest/GeneratedFiles" + clothingItemXMLDir + selectedItem + ".xml";
            else
                xmlFilePath = AppDomain.CurrentDomain.BaseDirectory + "/GeneratedFiles" + clothingItemXMLDir + selectedItem + ".xml";



            try
            {
                // Load the XML file
                XElement clothingItemElement = XElement.Load(xmlFilePath);

                // Map XML element names to corresponding textboxes
                var propertyToTextBoxMapping = new Dictionary<string, TextBox>
        {
            { "m_MaleModel", textBoxMaleModel },
            { "m_FemaleModel", textBoxFemaleModel },
            { "m_GUID", textBoxGUID },
            { "textureChoices", textBoxTextureChoices }
            // Add more mappings as needed
        };

                // Map XML element names to corresponding checkboxes
                var propertyToCheckboxMapping = new Dictionary<string, CheckBox>
        {
            { "m_Static", checkBoxStatic },
          //  { "m_AllowRandomHue", checkBoxAllowRandomHue },
          //  { "m_AllowRandomTint", checkBoxAllowRandomTint }
            // Add more checkboxes as needed
        };

                // Populate textboxes
                foreach (var property in propertyToTextBoxMapping)
                {
                    var element = clothingItemElement.Element(property.Key);
                    if (element != null)
                    {
                        property.Value.Text = element.Value;
                    }
                }

                // Populate checkboxes
                foreach (var kvp in propertyToCheckboxMapping)
                {
                    var element = clothingItemElement.Element(kvp.Key);
                    if (element != null)
                    {
                        string propertyValue = element.Value.Trim().ToLowerInvariant();

                        // Set the checkbox based on boolean values (true/false)
                        if (propertyValue == "true" || propertyValue == "1" || propertyValue == "yes" || propertyValue == "enabled")
                        {
                            kvp.Value.Checked = true;
                        }
                        else if (propertyValue == "false" || propertyValue == "0" || propertyValue == "no" || propertyValue == "disabled")
                        {
                            kvp.Value.Checked = false;
                        }
                        else
                        {
                            // Handle unexpected values if needed
                            Console.WriteLine($"Unexpected value for checkbox property '{kvp.Key}': {propertyValue}");
                            kvp.Value.Checked = false;
                        }
                    }
                    else
                    {
                        // If the property is not found, uncheck the checkbox
                        kvp.Value.Checked = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while populating controls from XML: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void ExtractVanilaItemDataToDictionary(string txtDirectoryPath, string xmlDirectory, string selectedItemName, Dictionary<string, (string ItemScript, string ItemXml)> itemDataDictionary)
        {
            try
            {
                // Ensure the directory exists
                if (!Directory.Exists(txtDirectoryPath))
                {
                    MessageBox.Show($"Directory '{txtDirectoryPath}' not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Get all .txt files in the directory
                string[] txtFiles = Directory.GetFiles(txtDirectoryPath, "*.txt");
                string itemScript = null;
                string itemXml = null;

                // Search for the item across all .txt files
                foreach (string txtFilePath in txtFiles)
                {
                    string fileContent = File.ReadAllText(txtFilePath);

                    // Match the selected item structure
                    Regex itemRegex = new Regex($@"item\s+{Regex.Escape(selectedItemName)}\s*\{{(.*?)\}}", RegexOptions.Singleline);
                    Match itemMatch = itemRegex.Match(fileContent);

                    if (itemMatch.Success)
                    {
                        itemScript = $"item {selectedItemName} {{\n{itemMatch.Groups[1].Value}\n}}";
                        break;
                    }
                }

                if (itemScript == null)
                {
                    MessageBox.Show($"Item '{selectedItemName}' not found in any file in the directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Extract associated XML data if ClothingItem property exists
                Regex clothingItemRegex = new Regex(@"ClothingItem\s*=\s*(?<value>[^,]+),", RegexOptions.IgnoreCase);
                Match clothingItemMatch = clothingItemRegex.Match(itemScript);

                if (clothingItemMatch.Success)
                {
                    string clothingItemFileName = clothingItemMatch.Groups["value"].Value.Trim();
                    string xmlFilePath = Path.Combine(xmlDirectory, $"{clothingItemFileName}.xml");

                    if (File.Exists(xmlFilePath))
                    {
                        itemXml = File.ReadAllText(xmlFilePath);
                    }
                    else
                    {
                        MessageBox.Show($"Associated XML file '{clothingItemFileName}.xml' not found.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                // Add the item script and XML data to the dictionary
                if (itemScript != null)
                {
                    itemDataDictionary[selectedItemName] = (itemScript, itemXml ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Dictionary<string, string> ReadItemPropertiesFromString(string itemScript, string selectedItem)
        {
            var properties = new Dictionary<string, string>();
            try
            {
                // Match the selected item structure within the provided script
                Regex itemRegex = new Regex($@"item\s+{Regex.Escape(selectedItem)}\s*\{{(.*?)\}}", RegexOptions.Singleline);
                Match itemMatch = itemRegex.Match(itemScript);

                if (!itemMatch.Success)
                {
                    throw new Exception($"Item '{selectedItem}' not found in the provided script.");
                }

                // Extract properties from the item structure
                string propertiesBlock = itemMatch.Groups[1].Value;

                // Match property name and value pairs
                Regex propertyRegex = new Regex(@"(?<name>\w+)\s*=\s*(?<value>[^,]+),");
                foreach (Match propertyMatch in propertyRegex.Matches(propertiesBlock))
                {
                    string propertyName = propertyMatch.Groups["name"].Value.Trim();
                    string propertyValue = propertyMatch.Groups["value"].Value.Trim();
                    properties[propertyName] = propertyValue;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return properties;
        }

        private void button8_Click(object sender, EventArgs e) //Vanila items window button
        {
            if (PZinstallPath.Length > 0)
            {
                ResetItemInfo();

                string selectedItem = listBoxVanila.SelectedItem.ToString();



                string txtDirectoryPath = PZinstallPath + clothingScriptsDir;
                string xmlDirectory = PZinstallPath + clothingItemXMLDir;

                var itemDataDictionary = new Dictionary<string, (string ItemScript, string ItemXml)>();

                ExtractVanilaItemDataToDictionary(txtDirectoryPath, xmlDirectory, selectedItem, itemDataDictionary);

                // Accessing the extracted data
                if (itemDataDictionary.TryGetValue(selectedItem, out var itemData))
                {
                    string itemScript = itemData.ItemScript;
                    string itemXml = itemData.ItemXml;

                    Console.WriteLine("Item Script:");
                    Console.WriteLine(itemScript);

                    Console.WriteLine("Item XML:");
                    Console.WriteLine(itemXml);


                    Dictionary<string, string> properties = ReadItemPropertiesFromString(itemScript, selectedItem);

                    if (properties != null)
                    {

                        //JSON
                        textBoxID.Text = selectedItem;
                        var propertyToTextBoxMapping = new Dictionary<string, TextBox>
            {
                { "DisplayName", textBoxDisplayName },
                { "DisplayCategory",textBoxCategory},
                { "Type", textBoxType },
                { "Weight",textBoxWeigth },
                { "IconsForTexture", textBoxTextureIcon },
                { "BodyLocation", textBoxBodyLocation },
                { "BloodLocation", textBoxBloodLocation },
                { "ChanceToFall", textBoxChanceToFall },
                { "ClothingItem", textBoxClothingItem },
                { "ClothingItemExtra", textBoxClothingItemExtra },
                { "ClothingItemExtraOption", textBoxClothingItemExtraOption },
                { "Insulation", textBoxInsulation },
                { "WindResistance", textBoxWind },
                { "FabricType", textBoxFabric },
                { "WorldStaticModel", textBoxStaticModel },
                { "Tags", textBoxTags },

                { "AttachmentReplacement", textBoxAttachReplacement},
                { "CanBeEquipped",textBoxCanbeequipped},
                { "Capacity",textBoxCapacity},
                { "OpenSound",textBoxOpenSound},
                { "CloseSound",textBoxCloseSound},
                { "PutInSound",textBoxPutinsound},
                { "ReplaceInPrimaryHand",textBoxReplaceprimaryhand},
                { "ReplaceInSecondHand", textBoxReplacesecondaryhand},
                { "RunSpeedModifier",textBoxRunspeedmodifier},
                { "WeightReduction",textBoxWeigthreduction},
                { "AttachmentsProvided",textBoxAttachmentProvided},

                            {"Icon", textBoxIcon },
                            {"BiteDefense", textBoxBiteDefense },
                            {"BulletDefense", textBoxBulletDefense },
                            {"ScratchDefense", textBoxScratchDefense },
                            {"DiscomfortModifier", textBoxDiscomfort },
                            {"CombatSpeedModifier", textBoxCombatSpeed },
                            {"WaterRessistance", textBoxWaterResistance },
                            {"VisionModifier", textBoxVisionModifier },
                            {"HearingModifier", textBoxHearing },
                            {"CorpseSicknessDefense", textBoxCorpseSicknessDefence },

                            {"DamageSound", textBoxDamageSound },
                            {"MaxItemSize", textBoxMaxItemSize },
                            {"SoundParameter", textBoxSoundParam },
                            {"MetalValue", textBoxMetalValue },
                            {"AcceptItemFunction", textBoxAcceptItemFunction },



               };

                        var propertyToCheckboxMapping = new Dictionary<string, CheckBox>
               {
                { "CanHaveHoles", checkBoxCanHaveHoles },
                { "ChanceToFall", checkBoxChanceToFall },
                { "ClothingItemExtra", checkBoxClothingItemExtra },
                { "ClothingItemExtraOption", checkBoxClothingItemExtraOption },
                { "Insulation", checkBoxInsulation },
                { "WindResistance", checkBoxWind },
                { "FabricType", checkBoxFabric },
                { "WorldStaticModel", checkBoxStaticModel },

                { "CanBeEquipped", checkBoxCanbeequipped},
                { "OpenSound", checkBoxOpensound},
                { "CloseSound", checkBoxClosesound},
                { "PutInSound", checkBoxPutinsound},
                { "ReplaceInPrimaryHand", checkBoxReplaceprimaryhand},
                { "ReplaceInSecondHand", checkBoxReplacesecondhand},
                { "RunSpeedModifier", checkBoxRunspeedmodifier},
                { "WeightReduction", checkBoxWeigthreduction},
                { "AttachmentsProvided",checkBoxAttachmentProvided},

                            {"AttachmentReplacement", checkBoxAttachmentReplacement },

                            {"Icon", checkBoxIcon },
                            {"BiteDefense", checkBoxBiteDefense },
                            {"BulletDefense", checkBoxBulletDefense },
                            {"ScratchDefense", checkBoxScratchDefense },
                            {"DiscomfortModifier", checkBoxDiscomfort },
                            {"CombatSpeedModifier", checkBoxCombatSpeed },
                            {"WaterResisstance", checkBoxWaterResistance },
                            {"VisionModifier", checkBoxVisionModifier },
                            {"HearingModifier", checkBoxHearingModifier },
                            {"CorpseSicknessDefense", checkBoxCorpseSicknessDefence },
                            {"Capacity", checkBoxCapacity },
                            {"DamageSound", checkBoxDamageSound },
                            {"MaxItemSize", checkBoxMaxItemSize },
                            {"SoundParameter", checkBoxSoundParam },
                            {"MetalValue", checkBoxMetalValue },
                            {"AcceptItemFunction", checkBoxAcceptItemFunction },

                            {"Cosmetic", checkBoxCosmetic },
                            {"VisualAid", checkBoxVisualAid },


                // Add more mappings as necessary
               };


                        // Populate textboxes
                        foreach (var property in properties)
                        {
                            if (propertyToTextBoxMapping.ContainsKey(property.Key))
                            {
                                propertyToTextBoxMapping[property.Key].Text = property.Value;
                            }
                        }

                        var stringValuesForCheckboxes = new HashSet<string>
            {
                "Enabled", "Active", "True", "On", "Yes", "TRUE"
                // Add any other string values that should trigger checkboxes to be checked
            };

                        // Populate checkboxes
                        foreach (var kvp in propertyToCheckboxMapping)
                        {
                            if (properties.ContainsKey(kvp.Key))
                            {
                                string propertyValue = properties[kvp.Key].Trim().ToLowerInvariant();

                                // Check for boolean values (true/false) and string-based true values
                                if (propertyValue == "true" || propertyValue == "1" || propertyValue == "yes" || propertyValue == "enabled")
                                {
                                    kvp.Value.Checked = true;
                                }
                                else if (propertyValue == "false" || propertyValue == "0" || propertyValue == "no" || propertyValue == "disabled")
                                {
                                    kvp.Value.Checked = false;
                                }
                                else if (float.TryParse(propertyValue, out float floatValue))
                                {
                                    // Property has a valid float value, enable the checkbox
                                    kvp.Value.Checked = true;
                                }
                                else if (!string.IsNullOrEmpty(propertyValue))
                                {
                                    // If the value is a non-empty string (e.g., "Cotton", "media/models/model.fbx"), enable the checkbox
                                    kvp.Value.Checked = true;
                                }
                                else
                                {
                                    // Unexpected value, log and uncheck the checkbox
                                    Console.WriteLine($"Unexpected value for property '{kvp.Key}': {propertyValue}");
                                    kvp.Value.Checked = false;
                                }
                            }
                            else
                            {
                                // Property is missing, set checkbox to false
                                kvp.Value.Checked = false;
                            }
                        }






                        //           string displayCategory = checkBoxCanHaveHoles.Checked ? textBoxDisplayName.Text : null;


                    }
                    //XML 

                    selectedItem = textBoxClothingItem.Text;

                    if (selectedItem.Length > 0)
                    {
                        string xmlFilePath = @"media/scripts/clothingItems/" + selectedItem;



                        xmlFilePath = PZinstallPath + clothingItemXMLDir + selectedItem + ".xml";



                        try
                        {
                            // Load the XML file
                            XElement clothingItemElement = XElement.Load(xmlFilePath);

                            // Map XML element names to corresponding textboxes
                            var propertyToTextBoxMapping = new Dictionary<string, TextBox>
        {
            { "m_MaleModel", textBoxMaleModel },
            { "m_FemaleModel", textBoxFemaleModel },
            { "m_GUID", textBoxGUID },
            { "textureChoices", textBoxTextureChoices }
            // Add more mappings as needed
        };

                            // Map XML element names to corresponding checkboxes
                            var propertyToCheckboxMapping = new Dictionary<string, CheckBox>
        {
            { "m_Static", checkBoxStatic },
          //  { "m_AllowRandomHue", checkBoxAllowRandomHue },
          //  { "m_AllowRandomTint", checkBoxAllowRandomTint }
            // Add more checkboxes as needed
        };

                            // Populate textboxes
                            foreach (var property in propertyToTextBoxMapping)
                            {
                                var element = clothingItemElement.Element(property.Key);
                                if (element != null)
                                {
                                    property.Value.Text = element.Value;
                                }
                            }

                            // Populate checkboxes
                            foreach (var kvp in propertyToCheckboxMapping)
                            {
                                var element = clothingItemElement.Element(kvp.Key);
                                if (element != null)
                                {
                                    string propertyValue = element.Value.Trim().ToLowerInvariant();

                                    // Set the checkbox based on boolean values (true/false)
                                    if (propertyValue == "true" || propertyValue == "1" || propertyValue == "yes" || propertyValue == "enabled")
                                    {
                                        kvp.Value.Checked = true;
                                    }
                                    else if (propertyValue == "false" || propertyValue == "0" || propertyValue == "no" || propertyValue == "disabled")
                                    {
                                        kvp.Value.Checked = false;
                                    }
                                    else
                                    {
                                        // Handle unexpected values if needed
                                        Console.WriteLine($"Unexpected value for checkbox property '{kvp.Key}': {propertyValue}");
                                        kvp.Value.Checked = false;
                                    }
                                }
                                else
                                {
                                    // If the property is not found, uncheck the checkbox
                                    kvp.Value.Checked = false;
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"An error occurred while populating controls from XML: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                        MessageBox.Show($"This is not valid clothing item, only regular item", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                    MessageBox.Show($"Project Zomboid not found on computer", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)  // Assemble GUID table
        {

            string outputPath;

            if (checkBoxDebug.Checked)
                outputPath = textBoxPath.Text;
            else
                outputPath = AppDomain.CurrentDomain.BaseDirectory;
            // Validate the output path


            try
            {

                outputPath += "/GeneratedFiles";
                // Create the output directory if it doesn't exist
                Directory.CreateDirectory(outputPath + GUIDsDir);

                string outputFilePath = outputPath + GUIDsDir + "newFileGuidTable.xml";

//                MessageBox.Show("C:/PZTest/GeneratedFiles" + clothingItemXMLDir, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                GenerateFileGuidTable(outputFilePath, "media/clothing/clothingItems/", listBoxItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while generating GUID Table", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void GenerateFileGuidTable(string outputFilePath, string folderPath, ListBox listBox)
        {
            try
            {
                // Define paths
                string inputFolderPath = @"C:/PZTest/GeneratedFiles/media/clothing/clothingitems/";
                string outputFilePathGUID = @"C:/PZTest/GeneratedFiles/media/newFileGuidTable.xml";

                // Initialize the root element of the XML
                var xmlDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
                var rootElement = new XElement("fileGuidTable");

                // Get all XML files in the folder
                string[] xmlFiles = Directory.GetFiles(inputFolderPath, "*.xml");

                foreach (string filePath in xmlFiles)
                {
                    try
                    {
                        Console.WriteLine($"Processing file: {filePath}");

                        // Read and parse the file
                        string fileContent = File.ReadAllText(filePath);
                        Console.WriteLine(fileContent); // Debug: Check the content

                        XDocument fileXml = XDocument.Parse(fileContent);

                        // Locate the <m_GUID> element
                        var guidElement = fileXml.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals("m_GUID", StringComparison.OrdinalIgnoreCase));

                        if (guidElement != null)
                        {
                            string guidValue = guidElement.Value.Trim();

                            // Extract the relative path starting from "media/"
                            int mediaIndex = filePath.IndexOf("media", StringComparison.OrdinalIgnoreCase);
                            string relativePath = mediaIndex >= 0 ? filePath.Substring(mediaIndex).Replace("\\", "/") : filePath;

                            // Create and add the entry for this file
                            var fileElement = new XElement("files",
                                new XElement("path", relativePath),
                                new XElement("guid", guidValue)
                            );

                            rootElement.Add(fileElement);
                        }
                        else
                        {
                            Console.WriteLine($"Warning: <m_GUID> not found in file: {filePath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
                    }
                }

                // Add root element and save the output file
                xmlDoc.Add(rootElement);
                xmlDoc.Save(outputFilePathGUID);

                MessageBox.Show("FileGuidTable.xml generated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the URL you want to open
                string url = "https://steamcommunity.com/id/peterhammerman/myworkshopfiles/";

                // Open the default web browser with the URL
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // Ensures it uses the default web browser
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while trying to open the web browser: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the URL you want to open
                string url = "https://skynet7500.wixsite.com/pzk-forge";

                // Open the default web browser with the URL
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // Ensures it uses the default web browser
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while trying to open the web browser: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the URL you want to open
                string url = "https://ko-fi.com/peterhammerman";

                // Open the default web browser with the URL
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // Ensures it uses the default web browser
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while trying to open the web browser: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the URL you want to open
                string url = "https://www.youtube.com/@PeterHammerman";

                // Open the default web browser with the URL
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // Ensures it uses the default web browser
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while trying to open the web browser: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the URL you want to open
                string url = "https://discord.gg/mPWXu5f";

                // Open the default web browser with the URL
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // Ensures it uses the default web browser
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while trying to open the web browser: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
    

   




