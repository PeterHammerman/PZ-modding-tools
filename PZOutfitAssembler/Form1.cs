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

namespace PZOutfitAssembler
{
    public partial class Form1 : Form
    {
        public string guid;
        public string itemID;
        // string directoryPath = AppDomain.CurrentDomain.BaseDirectory + "/GeneratedFiles/media/scripts/clothing/";
        public string directoryPath = "C:/PZTest/GeneratedFiles/media/scripts/clothing/";
        public string PZinstallPath = "";



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
                string vanillaItems = installPath + "/media/scripts/clothing/";
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
            // Get values from textboxes
            string IDName = "";
            string displayName = "";
            //           string displayCategory = checkBoxCanHaveHoles.Checked ? textBoxDisplayName.Text : null;
            string displayCategory = "";
            string type = "";
            string weight = "";
            string iconsForTexture = "";
            string bodyLocation = "";
            string bloodLocation = "";
            string chanceToFall = "";
            string clothingItem = "";
            string clothingItemExtra = "";
            string clothingItemExtraOption = "";
            string insulation = "";
            string wind = "";
            string fabrictype = "";
            string staticModel = "";
            string tags = "";

            string maleModel = "";
            string femaleModel = "";
            string textureChoices = "";

            bool isStatic = false;
            bool haveHoles = false;
            bool haveChanceToFall = false;
            bool haveClothingItemExtra = false;
            bool haveClothingItemExtraOptions = false;
            bool haveInsulation = false;
            bool haveWindRes = false;
            bool haveFabricType = false;
            bool haveWorldStaticModel = false;
            guid = "";


            textBoxID.Text = IDName;
            textBoxDisplayName.Text = displayName;
            //           string displayCategory = checkBoxCanHaveHoles.Checked ? textBoxDisplayName.Text : null;
            textBoxCategory.Text = displayCategory;
            textBoxType.Text = type;
            textBoxWeigth.Text = weight;
            textBoxTextureIcon.Text = iconsForTexture;
            textBoxBodyLocation.Text = bodyLocation;
            textBoxBloodLocation.Text = bloodLocation;
            textBoxChanceToFall.Text = chanceToFall;
            textBoxClothingItem.Text = clothingItem;
            textBoxClothingItemExtra.Text = clothingItemExtra;
            textBoxClothingItemExtraOption.Text = clothingItemExtraOption;
            textBoxInsulation.Text = insulation;
            textBoxWind.Text = wind;
            textBoxFabric.Text = fabrictype;
            textBoxStaticModel.Text = staticModel;
            textBoxTags.Text = tags;

            textBoxMaleModel.Text = maleModel;
            textBoxFemaleModel.Text = femaleModel;
            textBoxTextureChoices.Text = textureChoices;

            checkBoxStatic.Checked = isStatic;
            checkBoxCanHaveHoles.Checked = haveHoles;
            checkBoxChanceToFall.Checked = haveChanceToFall;
            checkBoxClothingItemExtra.Checked = haveClothingItemExtra;
            checkBoxClothingItemExtraOption.Checked = haveClothingItemExtraOptions;
            checkBoxInsulation.Checked = haveInsulation;
            checkBoxWind.Checked = haveWindRes;
            checkBoxFabric.Checked = haveFabricType;
            checkBoxStaticModel.Checked = haveWorldStaticModel;

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

        private void button1_Click(object sender, EventArgs e)
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
                    Directory.CreateDirectory(outputPath + "/media/scripts/clothing/");
                    Directory.CreateDirectory(outputPath + "/media/scripts/clothingItems/");
                    Directory.CreateDirectory(outputPath + "/media/clothing/");


                    // Assemble the content for itemname.txt
                    string itemNameContent = AssembleItemNameScript();

                    // Assemble the XML content
                    string xmlContent = AssembleItemXML();
                    string xmlnewOutfit = AssembleOutfitXML();
                    string xmlnewGUID = "test";


                    // Save the itemname.txt file
                    string itemNameFilePath = Path.Combine(outputPath + "/media/scripts/clothing/", itemID + ".txt");
                    File.WriteAllText(itemNameFilePath, itemNameContent);

                    string xmlClothItemFilePath = Path.Combine(outputPath + "/media/scripts/clothingItems/", itemID + ".xml");
                    File.WriteAllText(xmlClothItemFilePath, xmlContent);

                    string xmlGUIDTableFilePath = Path.Combine(outputPath + "/media/", "newFileGuidTable.xml");
                    File.WriteAllText(xmlGUIDTableFilePath, xmlnewGUID);

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
                filePath = AppDomain.CurrentDomain.BaseDirectory + "/GeneratedFiles//media/scripts/clothing/" + selectedItem + ".txt";



            populateItemEditorWindow(filePath, selectedItem);



        }

        private void button9_Click(object sender, EventArgs e)
        {
            ResetItemInfo();
        }

        private void populateItemEditorWindow(string filePath, string selectedItem)
        {
            Dictionary<string, string> properties = ReadItemProperties(filePath, selectedItem);

            if (properties != null)
            {
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


  //              textBoxMaleModel.Text = maleModel;
  //              textBoxFemaleModel.Text = femaleModel;
   //             textBoxTextureChoices.Text = textureChoices;
  //              checkBoxStatic.Checked = isStatic;

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



            string xmlFilePath = @"media/scripts/clothingItems/" + selectedItem;


            if (checkBoxDebug.Checked)
                xmlFilePath = "C:/PZTest/GeneratedFiles/media/scripts/clothingItems/" + selectedItem + ".xml";
            else
                xmlFilePath = AppDomain.CurrentDomain.BaseDirectory + "/GeneratedFiles/media/scripts/clothingItems/" + selectedItem + ".xml";



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

        private void button8_Click(object sender, EventArgs e)
        {
            if (PZinstallPath.Length > 0)
            {
                ResetItemInfo();

                string selectedItem = listBoxVanila.SelectedItem.ToString();
                string filePath = @"media/scripts/clothing/" + selectedItem;


                filePath = PZinstallPath + "/GeneratedFiles//media/scripts/clothing/" + selectedItem + ".txt";



                populateItemEditorWindow(filePath, selectedItem);
            }
            else
                MessageBox.Show($"Project ZOmboid not found on computer", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}




