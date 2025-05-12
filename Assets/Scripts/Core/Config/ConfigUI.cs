
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Core.Config
{
    public class ConfigUI
    {
        GameConfig GameConfig;
        VisualElement ConfigContent;
        UIManager UIManager;

        public ConfigUI(UIManager uIManager) { 
            UIManager = uIManager;
            ConfigContent = UIManager.Root.Q<VisualElement>("configContent");
            GameConfig = uIManager.GameConfig;
            
            GeneratePage();
        }

        private void GeneratePage()
        {
            var so = new SerializedObject(GameConfig);
            var iterator = so.GetIterator();
            iterator.NextVisible(true);

            ConfigContent.Clear();
            ConfigContent.style.flexDirection = FlexDirection.Row;
            ConfigContent.style.flexWrap = Wrap.NoWrap;

            // Create 3 vertical columns container
            var columns = new VisualElement[3]; // 3 columns
            for (int i = 0; i < 3; i++)
            {
                columns[i] = new VisualElement();
                columns[i].style.flexDirection = FlexDirection.Column;
                columns[i].style.marginRight = 16;
                columns[i].style.marginBottom = 8;  // Optional: For space between rows
                ConfigContent.Add(columns[i]);
            }

            // Collect all the fields into a list
            List<PropertyField> fields = new List<PropertyField>();
            while (iterator.NextVisible(false))
            {
                var field = new PropertyField(iterator);
                field.label = iterator.displayName;
                field.Bind(so);
                field.style.marginBottom = 8;
                fields.Add(field);
            }

            // Calculate number of rows
            int totalFields = fields.Count;
            int rows = Mathf.CeilToInt((float)totalFields / 3);

            // Add fields into columns in a column-major fashion
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    int fieldIndex = row + column * rows; // Calculate index of the current field for the column

                    // Ensure we don't exceed the number of fields
                    if (fieldIndex < totalFields)
                    {
                        columns[column].Add(fields[fieldIndex]);
                    }
                }
            }
        }


    }
}
