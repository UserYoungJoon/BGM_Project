using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace YoungJoon.Editor
{
    public static class EditorSheetImporter
    {
        public static string ExportUrl(string spreadsheetId, long gid)
            => $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/export?format=csv&gid={gid}";

        public static void Download(string url, Action<string> onText, Action<string> onError = null)
        {
            EditorCoroutineRunner.Start(DownloadRoutine(url, onText, onError));
        }

        private static IEnumerator DownloadRoutine(string url, Action<string> onText, Action<string> onError)
        {
            var request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
                request.Dispose();
                yield break;
            }

            string text = request.downloadHandler.text;
            request.Dispose();

            // 공유 막힌 시트는 200으로 로그인 HTML을 돌려줌 → 파싱하면 쓰레기가 조용히 들어감.
            if (LooksLikeLoginHtml(text))
            {
                onError?.Invoke("CSV가 아니라 HTML 반환 — 시트 공유를 '링크 있는 사용자 보기'로 했는지 확인");
                yield break;
            }

            try { onText?.Invoke(text); }
            catch (Exception e) { onError?.Invoke(e.Message); }
        }

        private static bool LooksLikeLoginHtml(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            string head = text.TrimStart();
            return head.StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase)
                || head.StartsWith("<html", StringComparison.OrdinalIgnoreCase);
        }

        public static List<List<string>> ParseCsv(string text)
        {
            if (!string.IsNullOrEmpty(text) && text[0] == '﻿') text = text.Substring(1);

            var rows = new List<List<string>>();
            var row = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"') { sb.Append('"'); i++; }
                        else inQuotes = false;
                    }
                    else sb.Append(c);
                }
                else
                {
                    if (c == '"') inQuotes = true;
                    else if (c == ',') { row.Add(sb.ToString()); sb.Clear(); }
                    else if (c == '\r') { }
                    else if (c == '\n') { row.Add(sb.ToString()); sb.Clear(); rows.Add(row); row = new List<string>(); }
                    else sb.Append(c);
                }
            }
            if (sb.Length > 0 || row.Count > 0) { row.Add(sb.ToString()); rows.Add(row); }
            return rows;
        }

        public static void SetRegistryArray<T>(UnityEngine.Object registry, string propertyName, IList<T> assets)
            where T : UnityEngine.Object
        {
            var so = new SerializedObject(registry);
            var prop = so.FindProperty(propertyName);
            prop.ClearArray();
            for (int i = 0; i < assets.Count; i++)
            {
                prop.InsertArrayElementAtIndex(i);
                prop.GetArrayElementAtIndex(i).objectReferenceValue = assets[i];
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(registry);
        }

        public static void EnsureFolder(string assetFolder)
        {
            assetFolder = assetFolder.TrimEnd('/');
            if (string.IsNullOrEmpty(assetFolder) || AssetDatabase.IsValidFolder(assetFolder)) return;
            string parent = Path.GetDirectoryName(assetFolder).Replace("\\", "/");
            string name = Path.GetFileName(assetFolder);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    public static class EditorCoroutineRunner
    {
        public static void Start(IEnumerator routine)
        {
            if (routine == null) return;
            new Driver(routine).Resume();
        }

        private sealed class Driver
        {
            private readonly Stack<IEnumerator> _stack = new Stack<IEnumerator>();

            public Driver(IEnumerator routine) => _stack.Push(routine);

            public void Resume() => EditorApplication.update += Tick;

            private void Tick()
            {
                while (_stack.Count > 0)
                {
                    var cur = _stack.Peek();
                    if (cur.Current is AsyncOperation op && !op.isDone) return;

                    bool moved;
                    try { moved = cur.MoveNext(); }
                    catch (Exception e)
                    {
                        Debug.LogError($"[EditorCoroutineRunner] 코루틴 예외: {e}");
                        EditorApplication.update -= Tick;
                        return;
                    }

                    if (!moved) { _stack.Pop(); continue; }
                    if (cur.Current is IEnumerator nested) { _stack.Push(nested); continue; }
                    return;
                }
                EditorApplication.update -= Tick;
            }
        }
    }
}
