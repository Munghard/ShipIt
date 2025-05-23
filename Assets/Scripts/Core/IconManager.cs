using System.Collections.Generic;
using UnityEngine;

public class IconManager
{
    // Constants
    private const int ICON_SIZE = 32;
    private const int SHEET_COLUMNS = 10;
    private const int SHEET_ROWS = 10;
    private const string SHEET_PATH = "textures/IconsShadow-32"; // Modify path based on your Unity project structure

    // Sprite sheet and icon list
    private Texture2D sheet;
    private List<IconData> iconList;

    // Store icon positions
    private Dictionary<int, Rect> icons;

    public void Load()
    {
        // Load the sprite sheet
        sheet = Resources.Load<Texture2D>(SHEET_PATH);
        if (sheet == null)
        {
            Debug.LogError("Failed to load sprite sheet from path: " + SHEET_PATH);
            return;
        }

        iconList = new List<IconData>();
        icons = new Dictionary<int, Rect>();

        // Generate icon positions
        GenerateIcons();

        // Load icon data (like the Lua example)
        LoadIcons();
    }

    // Function to generate icon positions
    private void GenerateIcons()
    {
        for (int row = 0; row < SHEET_ROWS; row++)
        {
            for (int col = 0; col < SHEET_COLUMNS; col++)
            {
                int index = row * SHEET_COLUMNS + col + 1;
                icons[index] = new Rect(col * ICON_SIZE, row * ICON_SIZE, ICON_SIZE, ICON_SIZE);
            }
        }
    }

    // Load icon data similar to Lua `load()` function
    private void LoadIcons()
    {
        iconList.Add(new IconData("Lock", GetIcon(83)));
    }

    // Get icon by name
    public IconData GetIconFromList(string name)
    {
        foreach (var icon in iconList)
        {
            if (icon.Name == name)
            {
                return icon;
            }
        }
        return null;
    }

    // Get the icon quad (in Unity, this is represented as a Rect)
    public IconData GetIcon(int index)
    {
        if (!icons.ContainsKey(index))
        {
            Debug.LogError("Invalid icon index: " + index);
            return null;
        }

        Rect iconRect = icons[index];
        IconData iconData = new IconData
        {
            Name = "Icon" + index,
            IconRect = iconRect,
            SheetTexture = sheet
        };

        return iconData;
    }

    // Class to hold icon data
    public class IconData
    {
        public string Name { get; set; }
        public Rect IconRect { get; set; }
        public Texture2D SheetTexture { get; set; }

        public IconData() { }

        public IconData(string name, IconData iconData)
        {
            Name = name;
            IconRect = iconData.IconRect;
            SheetTexture = iconData.SheetTexture;
        }
    }
}
