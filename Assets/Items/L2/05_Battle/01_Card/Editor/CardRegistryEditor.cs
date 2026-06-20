using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using YoungJoon.L2.Battle.Card;

namespace YoungJoon.Editor
{
    [CustomEditor(typeof(CardRegistry))]
    public class CardRegistryEditor : UnityEditor.Editor
    {
        private const string DataFolder = "Assets/Items/L2/05_Battle/01_Card/Datas";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            if (GUILayout.Button("시트 임포트", GUILayout.Height(32)))
                EditorSheetImporter.Download(SheetSources.CardMain,
                    csv => OnCsv((CardRegistry)target, csv),
                    err => Debug.LogError($"[CardImport] {err}"));
        }

        private void OnCsv(CardRegistry registry, string csv)
        {
            var rows = EditorSheetImporter.ParseCsv(csv);
            if (rows.Count < 2)
            {
                Debug.LogWarning("[CardImport] 데이터 행 없음 (헤더만 존재)");
                return;
            }

            var col = BuildHeaderMap(rows[0]);
            var assets = new List<CardDataSO>();

            for (int r = 1; r < rows.Count; r++)
            {
                if (TryReadCard(rows[r], col, out var data))
                    assets.Add(data);
            }

            EditorSheetImporter.SetRegistryArray(registry, "_cards", assets);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CardImport] {assets.Count}장 임포트 완료");
        }

        private static Dictionary<string, int> BuildHeaderMap(List<string> header)
        {
            var map = new Dictionary<string, int>();
            for (int i = 0; i < header.Count; i++)
            {
                var key = header[i].Trim();
                if (!string.IsNullOrEmpty(key)) map[key] = i;
            }
            return map;
        }

        private bool TryReadCard(List<string> row, Dictionary<string, int> col, out CardDataSO data)
        {
            data = null;
            if (!int.TryParse(Cell(row, col, "Id"), out int id))
                return false;

            var type = (CardType)id;
            if (!Enum.IsDefined(typeof(CardType), type))
            {
                Debug.LogWarning($"[CardImport] CardType 미정의 Id={id} → 스킵");
                return false;
            }

            string name = Cell(row, col, "한글이름");
            string tooltip = Cell(row, col, "Tooltip");
            int hp = ParseInt(Cell(row, col, "HP"));
            int cost = ParseInt(Cell(row, col, "Cost"));
            var effects = ParseEffect(Cell(row, col, "Effect"));

            data = LoadOrCreate(type, name);
            data.ReadData(type, name, tooltip, hp, cost, effects);
            return true;
        }

        private static CardDataSO LoadOrCreate(CardType type, string name)
        {
            string sub = type.GetCategory() == CardCategory.Skill ? "02_Skill" : "01_Attack";
            string dir = $"{DataFolder}/{sub}";
            EditorSheetImporter.EnsureFolder(dir);

            string path = $"{dir}/Card_{(int)type}_{Sanitize(name)}.asset";
            var data = AssetDatabase.LoadAssetAtPath<CardDataSO>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<CardDataSO>();
                AssetDatabase.CreateAsset(data, path);
            }
            return data;
        }

        private static string Cell(List<string> row, Dictionary<string, int> col, string key)
            => col.TryGetValue(key, out int i) && i < row.Count ? row[i].Trim() : string.Empty;

        private static int ParseInt(string s) => int.TryParse(s, out int v) ? v : 0;

        private static Dictionary<string, float> ParseEffect(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, float>();
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, float>>(json) ?? new Dictionary<string, float>();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[CardImport] Effect JSON 파싱 실패: {json} ({e.Message})");
                return new Dictionary<string, float>();
            }
        }

        private static string Sanitize(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Unnamed";
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
