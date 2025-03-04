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
using System.Xml.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Xml;




namespace PZOutfitAssembler
{

    public partial class Form1 : Form
    {
        public string guid;
        public string oGuid;
        public string itemID;
        // string directoryPath = AppDomain.CurrentDomain.BaseDirectory + "/GeneratedFiles/media/scripts/clothing/";
        public string directoryPath = "C:/PZTest/GeneratedFiles/media/scripts/clothing/";
        public string PZinstallPath = "";
        public string GUIDsDir = "/media/";
        public string clothingScriptsDir = "/media/scripts/clothing/";
        public string clothingXMLDir = "/media/clothing/";
        public string clothingItemXMLDir = "/media/clothing/clothingItems/";
        private ListBox activeListBox = null;


        public static class GlobalConfig //global variables
        {
            public static string VanillaPath { get; set; } = "";
            public static string ModdedPath { get; set; } = "";
        }


        public Form1()
        {
            string installPath = GetInstallLocation();
            if (string.IsNullOrEmpty(installPath))
            {
                MessageBox.Show($"Projezt Zomboid is not installed, cannot access registry path", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            InitializeComponent();
            activeListBox = listBoxMale;
            UpdateListBoxAppearance();



            PopulateItemListBoxWithFileNames(directoryPath, listBoxItems);

            if (!string.IsNullOrEmpty(installPath))
            {
                string vanillaItems = installPath + clothingScriptsDir;
                PZinstallPath = installPath;

                GlobalConfig.VanillaPath = installPath;
                GlobalConfig.ModdedPath = "C:/PZTest/GeneratedFiles";


                PopulateItemListBoxWithFileNames(vanillaItems, listBoxVanila);

                OutfitLoader cloader = new OutfitLoader(GlobalConfig.ModdedPath + clothingXMLDir + "clothing.xml"); //loading vanilla outfits
                cloader.PopulateListBox(listBoxCustomOutfit);

                OutfitLoader loader = new OutfitLoader(GlobalConfig.VanillaPath + clothingXMLDir + "clothing.xml"); //loading vanilla outfits
                loader.PopulateListBox(listBoxVanilaOutfit);
            }

            listBoxMale.SelectedIndexChanged += ListBoxMale_SelectedIndexChanged;
            listBoxFemale.SelectedIndexChanged += ListBoxFemale_SelectedIndexChanged;

          //  string aaa = Path.Combine(GlobalConfig.VanillaPath, "media", "clothing", "clothingItems");
         //   MessageBox.Show(aaa, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            searchDirectories[1] = Path.Combine(GlobalConfig.VanillaPath, "media", "clothing", "clothingItems");



        }

        private void buttonEditVanilaOutfit_Click(object sender, EventArgs e)
        {
            OutfitEditor editor = new OutfitEditor(PZinstallPath + clothingXMLDir + "clothing.xml", textBoxClothingName, textBoxOutfitGUID, textBoxOutfitFemaleGUID, checkBoxTop, checkBoxPants, checkBoxPantsHue, checkBoxPantsTint, checkBoxTopTint, checkBoxShirtDecal);
            editor.PopulateItemListBoxes(listBoxVanilaOutfit, listBoxMale, listBoxFemale);
        }

        public class OutfitEditor
        {
            private string xmlFilePath;
            private TextBox textBoxName;
            private TextBox textBoxGUID;
            private TextBox textBoxFemaleGUID;
            private CheckBox checkBoxTop;
            private CheckBox checkBoxPants;
            private CheckBox checkBoxAllowPantsHue;
            private CheckBox checkBoxAllowPantsTint;
            private CheckBox checkBoxAllowTopTint;
            private CheckBox checkBoxAllowTShirtDecal;

            public OutfitEditor(string path, TextBox nameBox, TextBox guidBox, TextBox guidFemaleBox, CheckBox topBox, CheckBox pantsBox,
                        CheckBox allowPantsHueBox, CheckBox allowPantsTintBox, CheckBox allowTopTintBox, CheckBox allowTShirtDecalBox)
            {
                xmlFilePath = path;
                textBoxName = nameBox;
                textBoxGUID = guidBox;
                textBoxFemaleGUID = guidFemaleBox;
                checkBoxTop = topBox;
                checkBoxPants = pantsBox;
                checkBoxAllowPantsHue = allowPantsHueBox;
                checkBoxAllowPantsTint = allowPantsTintBox;
                checkBoxAllowTopTint = allowTopTintBox;
                checkBoxAllowTShirtDecal = allowTShirtDecalBox;
            }

            private Dictionary<string, string> BuildGuidToNameDictionary()
            {
                Dictionary<string, string> guidToName = new Dictionary<string, string>();

                // Check both vanilla and modded paths
                string[] paths = { GlobalConfig.VanillaPath + "/media/clothing/clothingItems/", GlobalConfig.ModdedPath + "/media/clothing/clothingItems/" };

                foreach (string path in paths)
                {
                    if (!Directory.Exists(path)) continue; // Skip if path does not exist

                    foreach (string file in Directory.GetFiles(path, "*.xml"))
                    {
                        try
                        {
                            XDocument doc = XDocument.Load(file);
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                            // Search for GUIDs in XML
                            foreach (var guidElement in doc.Descendants("m_GUID"))
                            {
                                string guid = guidElement.Value.Trim();
                                if (!string.IsNullOrEmpty(guid) && !guidToName.ContainsKey(guid))
                                {
                                    guidToName[guid] = fileNameWithoutExtension;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error reading {file}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }

                return guidToName;
            }

            public void PopulateItemListBoxes(ListBox outfitListBox, ListBox listBoxMale, ListBox listBoxFemale)
            {
                if (!File.Exists(xmlFilePath))
                {
                    MessageBox.Show("XML file not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (outfitListBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select an outfit!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string selectedOutfit = outfitListBox.SelectedItem.ToString();
                Dictionary<string, string> guidToName = BuildGuidToNameDictionary();

                try
                {
                    XDocument doc = XDocument.Load(xmlFilePath);
                    var outfitElements = doc.Descendants("m_FemaleOutfits")
                                            .Concat(doc.Descendants("m_MaleOutfits"))
                                            .Where(o => (string)o.Element("m_Name") == selectedOutfit);

                    listBoxMale.Items.Clear();
                    listBoxFemale.Items.Clear();
                    textBoxFemaleGUID.Text = "";
                    textBoxGUID.Text = "";

                    foreach (var outfit in outfitElements)
                    {
                        bool isFemale = outfit.Name.LocalName == "m_FemaleOutfits";
                        var targetGuidBox = isFemale ? textBoxFemaleGUID : textBoxGUID;
                        var targetListBox = isFemale ? listBoxFemale : listBoxMale;

                        // Set GUID for appropriate gender
                        targetGuidBox.Text = (string)outfit.Element("m_Guid");

                        // Set common elements
                        textBoxName.Text = (string)outfit.Element("m_Name");

                        // Set checkboxes (keep as is)
                        checkBoxTop.Checked = outfit.Element("m_Top")?.Value.Trim().ToLower() == "true";
                        checkBoxPants.Checked = outfit.Element("m_Pants")?.Value.Trim().ToLower() == "true";
                        checkBoxAllowPantsHue.Checked = outfit.Element("m_AllowPantsHue")?.Value.Trim().ToLower() == "true";
                        checkBoxAllowPantsTint.Checked = outfit.Element("m_AllowPantsTint")?.Value.Trim().ToLower() == "true";
                        checkBoxAllowTopTint.Checked = outfit.Element("m_AllowTopTint")?.Value.Trim().ToLower() == "true";
                        checkBoxAllowTShirtDecal.Checked = outfit.Element("m_AllowTShirtDecal")?.Value.Trim().ToLower() == "true";

                        // Process items
                        foreach (var item in outfit.Elements("m_items"))
                        {
                            string itemGuid = item.Element("itemGUID")?.Value;
                            string probability = item.Element("probability")?.Value;

                            if (!string.IsNullOrEmpty(itemGuid))
                            {
                                string displayName = guidToName.ContainsKey(itemGuid) ? guidToName[itemGuid] : itemGuid;
                                string itemText = !string.IsNullOrEmpty(probability) ? $"{displayName} ({probability})" : displayName;
                                targetListBox.Items.Add(itemText);
                            }

                            // Process subitems
                            foreach (var subItem in item.Elements("subItems").Elements("itemGUID"))
                            {
                                string subItemGuid = subItem.Value;
                                if (!string.IsNullOrEmpty(subItemGuid))
                                {
                                    string displaySubItemName = guidToName.ContainsKey(subItemGuid) ? guidToName[subItemGuid] : subItemGuid;
                                    targetListBox.Items.Add($"<>{displaySubItemName}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading XML: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // Helper function to properly parse boolean values
            private bool ParseBool(string value)
            {
                return value?.Trim().ToLower() == "true";
            }
        }

        public class OutfitLoader  //loading vanilla outfits
        {
            private string xmlFilePath;
            private Dictionary<string, string> guidToFilenameMap = new Dictionary<string, string>();
            string vanillaPath = GlobalConfig.VanillaPath + "/media/clothing/clothingItems/";
            string moddedPath = GlobalConfig.ModdedPath + "/media/clothing/clothingItems/";

            public OutfitLoader(string path)
            {
                xmlFilePath = path;
                LoadGuidMappings(vanillaPath, moddedPath);
            }

            private void LoadGuidMappings(string vanillaPath, string moddedPath)
            {
                guidToFilenameMap.Clear();
                LoadGuidsFromPath(vanillaPath);
                LoadGuidsFromPath(moddedPath);
            }

            private void LoadGuidsFromPath(string folderPath)
            {
                if (!Directory.Exists(folderPath))
                    return;

                foreach (string file in Directory.GetFiles(folderPath, "*.xml", SearchOption.AllDirectories))
                {
                    try
                    {
                        XDocument doc = XDocument.Load(file);
                        var guidElement = doc.Descendants("m_GUID").FirstOrDefault();

                        if (guidElement != null)
                        {
                            string guid = guidElement.Value.Trim();
                            string fileName = Path.GetFileNameWithoutExtension(file);

                            if (!guidToFilenameMap.ContainsKey(guid))
                            {
                                guidToFilenameMap[guid] = fileName;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error reading {file}: {ex.Message}");
                    }
                }
            }



            public void PopulateListBox(ListBox listBox)
            {
                if (!File.Exists(xmlFilePath) && listBox.Name == "listBoxVanilaOutfit")
                {
                    MessageBox.Show("vanilla outfit XML file not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (!File.Exists(xmlFilePath) && listBox.Name != "listBoxVanilaOutfit")
                {
                    return;
                }

                try
                {
                    HashSet<string> addedNames = new HashSet<string>(); // Changed to track names
                    List<string> outfits = new List<string>();

                    XDocument doc = XDocument.Load(xmlFilePath);

                    var outfitElements = doc.Descendants("m_FemaleOutfits")
                                          .Concat(doc.Descendants("m_MaleOutfits"));

                    foreach (var outfit in outfitElements)
                    {
                        string name = outfit.Element("m_Name")?.Value;
                        string guid = outfit.Element("m_Guid")?.Value;

                        if (!string.IsNullOrEmpty(name) &&
                            !string.IsNullOrEmpty(guid) &&
                            addedNames.Add(name)) // Check name uniqueness
                        {
                            outfits.Add(name);
                        }
                    }

                    listBox.Items.Clear();
                    listBox.Items.AddRange(outfits.ToArray());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading XML: {ex.Message}", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
            textBoxaltFemaleModel.Text = "";
            textBoxaltMaleModel.Text = "";
            textBoxMasks.Text = "";
            textBoxunderlayMasksFolder.Text = "";
            textBoxmasksFolder.Text = "";



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
            checkBoxAllowRandomHue.Checked = false;
            checkBoxAllowRandomTint.Checked = false;



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

        private void button1_Click(object sender, EventArgs e) //Save new item
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
                    //   string xmlnewOutfit = AssembleOutfitXML();



                    // Save the itemname.txt file
                    string itemNameFilePath = Path.Combine(outputPath + clothingScriptsDir, itemID + ".txt");
                    File.WriteAllText(itemNameFilePath, itemNameContent);

                    string xmlClothItemFilePath = Path.Combine(outputPath + clothingItemXMLDir, textBoxClothingItem.Text + ".xml");
                    File.WriteAllText(xmlClothItemFilePath, xmlContent);



                    //   string xmlOutfitTableFilePath = Path.Combine(outputPath + "/media/clothing/", "newClothing.xml");
                    //   File.WriteAllText(xmlOutfitTableFilePath, xmlnewOutfit);

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
            script += $"/* created by PZ Clothing Editor - PZK Forge - Peter Hammerman */\n";
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

            if (checkBoxCanbeequipped.Checked)
                script += $"        CanBeEquipped = {canbeequipped},\n";
            if (checkBoxCapacity.Checked)
                script += $"        Capacity = {capacity},\n";

            if (checkBoxOpensound.Checked)
                script += $"        OpenSound = {opensound},\n";
            if (checkBoxClosesound.Checked)
                script += $"        CloseSound = {closesound},\n";
            if (checkBoxPutinsound.Checked)
                script += $"        PutInSound = {putinsound},\n";
            if (checkBoxReplaceprimaryhand.Checked)
                script += $"        ReplaceInPrimaryHand = {primaryhand},\n";
            if (checkBoxReplacesecondhand.Checked)
                script += $"        ReplaceInSecondHand = {secondaryhand},\n";
            if (checkBoxRunspeedmodifier.Checked)
                script += $"        RunSpeedModifier = {runspeedmodifier},\n";
            if (checkBoxWeigthreduction.Checked)
                script += $"        WeightReduction = {weigthreduction},\n";
            if (checkBoxAttachmentProvided.Checked)
                script += $"        AttachmentsProvided = {attachmentprovided},\n";

            if (checkBoxIcon.Checked)
                script += $"        Icon = {icon},\n";
            if (checkBoxBiteDefense.Checked)
                script += $"        BiteDefense = {bitedefense},\n";

            if (checkBoxBulletDefense.Checked)
                script += $"        BulletDefense = {bulletdef},\n";
            if (checkBoxScratchDefense.Checked)
                script += $"        ScratchDefense = {scratchdef},\n";
            if (checkBoxDiscomfort.Checked)
                script += $"        DiscomfortModifier = {discomfort},\n";
            if (checkBoxCombatSpeed.Checked)
                script += $"        CombatSpeedModifier = {combatspeed},\n";
            if (checkBoxWaterResistance.Checked)
                script += $"        WaterRessistance = {waterres},\n";
            if (checkBoxVisionModifier.Checked)
                script += $"        VisionModifier = {visionmod},\n";
            if (checkBoxHearingModifier.Checked)
                script += $"        HearingModifier = {hearing},\n";
            if (checkBoxCorpseSicknessDefence.Checked)
                script += $"        CorpseSicknessDefense = {corpsedef},\n";

            if (checkBoxDamageSound.Checked)
                script += $"        DamageSound = {damagesound},\n";
            if (checkBoxMaxItemSize.Checked)
                script += $"        MaxItemSize = {maxitemsize},\n";
            if (checkBoxSoundParam.Checked)
                script += $"        SoundParameter = {soundparam},\n";
            if (checkBoxMetalValue.Checked)
                script += $"        MetalValue = {metalvalue},\n";

            if (checkBoxAcceptItemFunction.Checked)
                script += $"        AcceptItemFunction = {itemfunction},\n";

            if (checkBoxCosmetic.Checked)
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
            string altmaleModel = textBoxaltMaleModel.Text;
            string altfemaleModel = textBoxaltFemaleModel.Text;
            string textureChoices = textBoxTextureChoices.Text;
            string masks = textBoxMasks.Text;
            string masksFolder = textBoxmasksFolder.Text;
            string underlayMasksFolder = textBoxunderlayMasksFolder.Text;
            bool isStatic = checkBoxStatic.Checked;

            // Build the XML content
            StringBuilder xml = new StringBuilder();

            xml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xml.AppendLine("<!-- Script generated by PZ Outfit Assembler | PZK Forge - Peter Hammerman | https://github.com/PeterHammerman/PZ-modding-tools -->");
            xml.AppendLine("<clothingItem>");
            xml.AppendLine($"  <m_MaleModel>{maleModel}</m_MaleModel>");
            xml.AppendLine($"  <m_FemaleModel>{femaleModel}</m_FemaleModel>");

            if (!string.IsNullOrEmpty(altmaleModel))
                xml.AppendLine($"  <m_AltMaleModel>{altmaleModel}</m_AltMaleModel>");
            if (!string.IsNullOrEmpty(altfemaleModel))
                xml.AppendLine($"  <m_AltFemaleModel>{altfemaleModel}</m_AltFemaleModel>");

            xml.AppendLine($"  <m_GUID>{guid}</m_GUID>");
            xml.AppendLine($"  <m_Static>{isStatic}</m_Static>");
            xml.AppendLine($"  <m_AllowRandomHue>{checkBoxAllowRandomHue.Checked}</m_AllowRandomHue>");
            xml.AppendLine($"  <m_AllowRandomTint>{checkBoxAllowRandomTint.Checked}</m_AllowRandomTint>");
            xml.AppendLine("  <m_AttachBone></m_AttachBone>");

            if (!string.IsNullOrEmpty(maleModel) || !string.IsNullOrEmpty(femaleModel))
                xml.AppendLine($"  <textureChoices>{textureChoices}</textureChoices>");
            else
                xml.AppendLine($"  <m_BaseTextures>{textureChoices}</m_BaseTextures>");

            // Handle multiple <m_Masks> properties
            if (!string.IsNullOrEmpty(masks))
            {
                var maskValues = masks.Split(',')
                                      .Select(m => m.Trim())
                                      .Where(m => !string.IsNullOrEmpty(m));

                foreach (var mask in maskValues)
                {
                    xml.AppendLine($"  <m_Masks>{mask}</m_Masks>");
                }
            }

            if (!string.IsNullOrEmpty(masksFolder))
                xml.AppendLine($"  <m_MasksFolder>{masksFolder}</m_MasksFolder>");

            if (!string.IsNullOrEmpty(underlayMasksFolder))
                xml.AppendLine($"  <m_UnderlayMasksFolder>{underlayMasksFolder}</m_UnderlayMasksFolder>");

            xml.AppendLine("</clothingItem>");

            return xml.ToString();
        }

        /*
        private string AssembleOutfitXML()
        {
            // Get values from textboxes
           // string probability = textBoxProbability.Text;
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
        */

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
                XDocument xmlDoc = XDocument.Load(xmlFilePath);
                var masks = xmlDoc.Descendants("m_Masks")
                                  .Select(x => x.Value.Trim())
                                  .Where(x => !string.IsNullOrEmpty(x))
                                  .ToList();

                textBoxMasks.Text = string.Join(",", masks);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading XML: " + ex.Message);
            }

            try
            {
                // Load the XML file
                XElement clothingItemElement = XElement.Load(xmlFilePath);

                // Map XML element names to corresponding textboxes
                var propertyToTextBoxMapping = new Dictionary<string, TextBox>
        {
            { "m_MaleModel", textBoxMaleModel },
            { "m_FemaleModel", textBoxFemaleModel },
            { "m_AltMaleModel", textBoxaltMaleModel },
            { "m_AltFemaleModel", textBoxaltFemaleModel },
            { "m_GUID", textBoxGUID },
            { "textureChoices", textBoxTextureChoices },
            { "m_BaseTextures", textBoxTextureChoices },
       //     { "m_Masks", textBoxMasks },
            { "m_MasksFolder", textBoxmasksFolder },
            { "m_UnderlayMasksFolder", textBoxunderlayMasksFolder }

            // Add more mappings as needed
        };

                // Map XML element names to corresponding checkboxes
                var propertyToCheckboxMapping = new Dictionary<string, CheckBox>
        {
            { "m_Static", checkBoxStatic },
            {"m_AllowRandomHue", checkBoxAllowRandomHue },
            {"m_AllowRandomTint", checkBoxAllowRandomTint},

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
                            XDocument xmlDoc = XDocument.Load(xmlFilePath);
                            var masks = xmlDoc.Descendants("m_Masks")
                                              .Select(x => x.Value.Trim())
                                              .Where(x => !string.IsNullOrEmpty(x))
                                              .ToList();

                            textBoxMasks.Text = string.Join(",", masks);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error reading XML: " + ex.Message);
                        }


                        try
                        {
                            // Load the XML file
                            XElement clothingItemElement = XElement.Load(xmlFilePath);

                            // Map XML element names to corresponding textboxes
                            var propertyToTextBoxMapping = new Dictionary<string, TextBox>
                            {
                                  { "m_MaleModel", textBoxMaleModel },
                                  { "m_FemaleModel", textBoxFemaleModel },
                                  { "m_AltMaleModel", textBoxaltMaleModel },
                                  { "m_AltFemaleModel", textBoxaltFemaleModel },
                                  { "m_GUID", textBoxGUID },
                                  { "textureChoices", textBoxTextureChoices },
                                  { "m_BaseTextures", textBoxTextureChoices },
                              //    { "m_Masks", textBoxMasks },
                                  { "m_MasksFolder", textBoxmasksFolder },
                                  { "m_UnderlayMasksFolder", textBoxunderlayMasksFolder }
            // Add more mappings as needed
                            };

                            // Map XML element names to corresponding checkboxes
                            var propertyToCheckboxMapping = new Dictionary<string, CheckBox>
                            {
                                  {"m_Static", checkBoxStatic },
                                  {"m_AllowRandomHue", checkBoxAllowRandomHue},
                                  {"m_AllowRandomTint", checkBoxAllowRandomTint},

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
                Console.WriteLine($"An error occurred while generating GUID Table: {ex.Message}");
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

        private void buttonOutfitClear_Click(object sender, EventArgs e)
        {
            listBoxMale.Items.Clear();
            listBoxFemale.Items.Clear();
        }

        private void ListBoxMale_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ListBoxFemale_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button12_Click(object sender, EventArgs e)  //Remove selected button
        {
            // Remove selected item from listBoxMale if something is selected
            if (listBoxMale.SelectedIndex != -1)
            {
                listBoxMale.Items.RemoveAt(listBoxMale.SelectedIndex);
                return;
            }

            // Remove selected item from listBoxFemale if something is selected
            if (listBoxFemale.SelectedIndex != -1)
            {
                listBoxFemale.Items.RemoveAt(listBoxFemale.SelectedIndex);
            }
        }

        private void button14_Click(object sender, EventArgs e)  // Generate GUID for outfit
        {
            oGuid = GenerateGuid();
            textBoxOutfitGUID.Text = oGuid;
            oGuid = GenerateGuid();
            textBoxOutfitFemaleGUID.Text = oGuid;
        }



        private void button3_Click(object sender, EventArgs e) //Add from created items database
        {
            EnsureListBoxSelected();
            if (listBoxItems.SelectedItem != null)
            {
                AddItemToSelectedListBox(listBoxItems.SelectedItem.ToString(), listBoxItems, textBoxProb1.Text);
            }
        }

        private void button6_Click(object sender, EventArgs e) //Add from vanilla item database
        {
            EnsureListBoxSelected();
            if (listBoxVanila.SelectedItem != null)
            {
                AddItemToSelectedListBox(listBoxVanila.SelectedItem.ToString(), listBoxVanila, textBoxProb2.Text);
            }
        }

        private void AddItemToSelectedListBox(string itemText, ListBox inputListbox, string prob)
        {
            if (activeListBox == null)
            {
                MessageBox.Show("Please select either Male or Female list first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (listBoxVanila.SelectedItem == null && listBoxItems.SelectedItem == null)
            {
                MessageBox.Show("Please select an item to add!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get selected item from either listBoxVanilla or listBoxItems
            string selectedItem = inputListbox.SelectedItem?.ToString();

            if (prob != "1" && prob != "")
            {
                selectedItem += " (" + prob + ")";
            }

            if (!string.IsNullOrEmpty(selectedItem))
            {
                // Check if activeListBox is empty, then just add the item
                if (activeListBox.Items.Count == 0)
                {
                    activeListBox.Items.Add(selectedItem);
                }
                else
                {
                    // Otherwise, add below the currently selected item
                    int selectedIndex = activeListBox.SelectedIndex;
                    if (selectedIndex >= 0 && selectedIndex < activeListBox.Items.Count - 1)
                    {
                        activeListBox.Items.Insert(selectedIndex + 1, selectedItem);
                    }
                    else
                    {
                        activeListBox.Items.Add(selectedItem);
                    }
                }
            }
        }

        private void AddAltItemToSelectedListBox(string itemText, ListBox inputListbox)
        {
            if (activeListBox == null)
            {
                MessageBox.Show("Please select either Male or Female list first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (listBoxVanila.SelectedItem == null && listBoxItems.SelectedItem == null)
            {
                MessageBox.Show("Please select an item to add!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get selected item from either listBoxVanilla or listBoxItems
            string selectedItem = "<>"+inputListbox.SelectedItem?.ToString();



            if (!string.IsNullOrEmpty(selectedItem))
            {
                // Check if activeListBox is empty, then just add the item
                if (activeListBox.Items.Count == 0)
                {
                    activeListBox.Items.Add(selectedItem);
                }
                else
                {
                    // Otherwise, add below the currently selected item
                    int selectedIndex = activeListBox.SelectedIndex;
                    if (selectedIndex >= 0 && selectedIndex < activeListBox.Items.Count - 1)
                    {
                        activeListBox.Items.Insert(selectedIndex + 1, selectedItem);
                    }
                    else
                    {
                        activeListBox.Items.Add(selectedItem);
                    }
                }
            }
        }

        private void UpdateListBoxAppearance()
        {
            listBoxMale.BackColor = (activeListBox == listBoxMale) ? SystemColors.Window : SystemColors.InactiveBorder;
            listBoxFemale.BackColor = (activeListBox == listBoxFemale) ? SystemColors.Window : SystemColors.InactiveBorder;
        }

        private void EnsureListBoxSelected()
        {
            if (activeListBox == null)
            {
                MessageBox.Show("Please select either Male or Female list!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void listBoxMale_Click(object sender, EventArgs e)
        {
            if (listBoxFemale.SelectedIndex != -1) // Only switch if an item is selected
            {
                activeListBox = listBoxMale;
                listBoxFemale.ClearSelected();
                UpdateListBoxAppearance();
                //   MessageBox.Show("listBoxMale_Click", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                activeListBox = listBoxMale;
                UpdateListBoxAppearance();
            }

        }

        private void listBoxFemale_Click(object sender, EventArgs e)
        {
            if (listBoxMale.SelectedIndex != -1) // Only switch if an item is selected
            {
                activeListBox = listBoxFemale;
                listBoxMale.ClearSelected();
                UpdateListBoxAppearance();
                //   MessageBox.Show("listBoxFeMale_Click", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                activeListBox = listBoxFemale;
                UpdateListBoxAppearance();
            }

        }

        private void listBoxMale_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            activeListBox = listBoxMale;
            if (listBoxMale.SelectedIndex != -1) // Only switch if an item is selected
            {
                listBoxFemale.ClearSelected();
            }
            UpdateListBoxAppearance();
            //   MessageBox.Show("listBoxMale_SelectedIndexChanged_1", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void listBoxFemale_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            activeListBox = listBoxFemale;
            if (listBoxFemale.SelectedIndex != -1) // Only switch if an item is selected
            {
                listBoxMale.ClearSelected();
            }
            UpdateListBoxAppearance();
            //   MessageBox.Show("listBoxFemale_SelectedIndexChanged_1", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // Define your search directories for item XML files
        public string[] searchDirectories = {
            @"C:\PZTest\GeneratedFiles\media\scripts\clothing\clothingItems",
 //           @"H:\Program Files (x86)\SteamLibrary\steamapps\common\ProjectZomboid\media\clothing\clothingItems",
            Path.Combine(GlobalConfig.VanillaPath, "media", "clothing", "clothingItems")


        };


        private void buttonSaveOutfit_Click(object sender, EventArgs e)
        {
            string outputDirectory = @"C:\PZTest\GeneratedFiles\media\clothing";
            string fileName = "clothing.xml";
            string fullPath = Path.Combine(outputDirectory, fileName);

            // Check if both listboxes are empty
            if (listBoxFemale.Items.Count == 0 && listBoxMale.Items.Count == 0)
            {
                MessageBox.Show("Both male and female outfits are empty!\nNothing to save.",
                               "Error",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
                return;
            }

            Directory.CreateDirectory(outputDirectory);

            // Create outfit elements only for non-empty listboxes


            var genderOutfits = new List<XElement>();

            if (listBoxFemale.Items.Count > 0)
            {
                genderOutfits.Add(CreateGenderOutfits("Female", listBoxFemale, textBoxOutfitFemaleGUID.Text));
            }

            if (listBoxMale.Items.Count > 0)
            {
                genderOutfits.Add(CreateGenderOutfits("Male", listBoxMale, textBoxOutfitGUID.Text));
            }

            XDocument finalDoc;
            var newOutfitElements = new XElement("outfitManager", genderOutfits).Elements();
            var comment = new XComment(" Script generated by PZ Outfit Assembler | PZK Forge - Peter Hammerman | https://github.com/PeterHammerman/PZ-modding-tools ");

            // Merge logic
            if (File.Exists(fullPath))
            {
                finalDoc = XDocument.Load(fullPath);

                // Remove existing version of our comment if present
                var existingComments = finalDoc.DescendantNodes()
                    .OfType<XComment>()
                    .Where(c => c.Value.Trim() == comment.Value.Trim())
                    .ToList();

                foreach (var c in existingComments)
                {
                    c.Remove();
                }

                var newNames = genderOutfits
                .Select(x => x.Element("m_Name")?.Value)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .ToList();

                finalDoc.Root.Elements()
                    .Where(el => newNames.Contains(el.Element("m_Name")?.Value))
                    .Remove();

                finalDoc.Root.Add(newOutfitElements);

                // Add new comment at the top
                finalDoc.AddFirst(comment);
            }
            else
            {
                finalDoc = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    comment,
                    new XElement("outfitManager", newOutfitElements)
                );
            }

            // Save with formatting
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                Encoding = Encoding.UTF8
            };

            using (var writer = XmlWriter.Create(fullPath, settings))
            {
                finalDoc.Save(writer);
            }

            // Refresh ListBox
            OutfitLoader cloader = new OutfitLoader(fullPath);
            cloader.PopulateListBox(listBoxCustomOutfit);
            MessageBox.Show("XML file saved successfully!");
        }

        private XElement CreateGenderOutfits(string gender, ListBox listBox, string guid)
        {
            return new XElement($"m_{gender}Outfits",
                new XElement("m_Name", textBoxClothingName.Text),
                new XElement("m_Guid", guid),
                new XElement("m_Top", checkBoxTop.Checked.ToString().ToLower()),
                new XElement("m_Pants", checkBoxPants.Checked.ToString().ToLower()),
                new XElement("m_AllowPantsHue", checkBoxPantsHue.Checked.ToString().ToLower()),
                new XElement("m_AllowTopTint", checkBoxTopTint.Checked.ToString().ToLower()),
                new XElement("m_AllowTShirtDecal", checkBoxShirtDecal.Checked.ToString().ToLower()),
                ProcessListBoxItemsProperly(listBox)
            );
        }

        private IEnumerable<XElement> ProcessListBoxItemsProperly(ListBox listBox)
        {
            var items = new List<OutfitItem>();
            OutfitItem currentItem = null;

            foreach (var item in listBox.Items)
            {
                var line = item.ToString();
                if (line.StartsWith("<>"))
                {
                    currentItem?.SubItems.Add(line.Substring(2).Trim());
                }
                else
                {
                    currentItem = ParseItemLine(line);
                    items.Add(currentItem);
                }
            }

            foreach (var item in items)
            {
                var itemElement = new XElement("m_items");
                var mainGuid = FindItemGuid(item.Name);

                if (string.IsNullOrEmpty(mainGuid))
                {
                    MessageBox.Show($"GUID not found for item: {item.Name}");
                    continue;
                }

                if (item.Probability.HasValue && item.Probability < 1.0)
                {
                    itemElement.Add(new XElement("probability",
                        item.Probability.Value.ToString("0.0##", CultureInfo.InvariantCulture)));
                }

                itemElement.Add(new XElement("itemGUID", mainGuid));

                foreach (var subItem in item.SubItems)
                {
                    var subGuid = FindItemGuid(subItem);
                    if (!string.IsNullOrEmpty(subGuid))
                    {
                        itemElement.Add(new XElement("subItems",
                            new XElement("itemGUID", subGuid)
                        ));
                    }
                }

                yield return itemElement;
            }
        }

        private string FindItemGuid(string itemName)
        {
            foreach (var dir in searchDirectories)
            {
                var xmlPath = Path.Combine(dir, $"{itemName}.xml");
                if (File.Exists(xmlPath))
                {
                    try
                    {
                        var doc = XDocument.Load(xmlPath);
                        return doc.Root?.Element("m_GUID")?.Value; // Fixed element name
                    }
                    catch { /* Add error logging */ }
                }
            }
            return null;
        }

        // Rest of the class remains the same


        private OutfitItem ParseItemLine(string line)
        {
            var item = new OutfitItem();
            var match = Regex.Match(line, @"^(.*?)(?:\s+\(([0-9.,]+)\))?$");

            if (match.Success)
            {
                item.Name = match.Groups[1].Value.Trim();

                if (match.Groups[2].Success)
                {
                    var numberString = match.Groups[2].Value
                        .Replace(',', '.');

                    if (float.TryParse(numberString, NumberStyles.Any,
                        CultureInfo.InvariantCulture, out float prob))
                    {
                        item.Probability = prob;
                    }
                }
            }
            return item;
        }

        private void button11_Click(object sender, EventArgs e)  //Delete outfit entry
        {
            if (listBoxCustomOutfit.SelectedItem == null)
            {
                MessageBox.Show("Please select an outfit to delete");
                return;
            }

            string outputDirectory = @"C:\PZTest\GeneratedFiles\media\clothing";
            string fileName = "clothing.xml";
            string fullPath = Path.Combine(outputDirectory, fileName);

            var selectedName = listBoxCustomOutfit.SelectedItem.ToString();

            try
            {
                XDocument doc = XDocument.Load(fullPath);

                // Find both male and female versions
                var toDelete = doc.Root.Elements()
                    .Where(el => el.Element("m_Name")?.Value == selectedName)
                    .ToList();

                if (toDelete.Count == 0)
                {
                    MessageBox.Show("Outfit not found in XML file");
                    return;
                }

                // Remove found elements
                toDelete.Remove();

                // Save changes
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "    ",
                    Encoding = Encoding.UTF8
                };

                using (var writer = XmlWriter.Create(fullPath, settings))
                {
                    doc.Save(writer);
                }

                // Refresh list
                OutfitLoader cloader = new OutfitLoader(fullPath);
                cloader.PopulateListBox(listBoxCustomOutfit);
                MessageBox.Show("Outfit deleted successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting outfit: {ex.Message}");
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            OutfitEditor editor = new OutfitEditor(GlobalConfig.ModdedPath + clothingXMLDir + "clothing.xml", textBoxClothingName, textBoxOutfitGUID, textBoxOutfitFemaleGUID, checkBoxTop, checkBoxPants, checkBoxPantsHue, checkBoxPantsTint, checkBoxTopTint, checkBoxShirtDecal);
            editor.PopulateItemListBoxes(listBoxCustomOutfit, listBoxMale, listBoxFemale);
        }

        private void button18_Click(object sender, EventArgs e)
        {
            EnsureListBoxSelected();
            if (listBoxItems.SelectedItem != null)
            {
                AddAltItemToSelectedListBox(listBoxItems.SelectedItem.ToString(), listBoxItems);
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            EnsureListBoxSelected();
            if (listBoxVanila.SelectedItem != null)
            {
                AddAltItemToSelectedListBox(listBoxVanila.SelectedItem.ToString(), listBoxVanila);
            }
        }
    }


    public class OutfitItem
    {
        public string Name { get; set; }
        public float? Probability { get; set; }
        public List<string> SubItems { get; } = new List<string>();
    }
}








