using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*

namespace uFileBrowser
{
    public delegate void FileBrowserCallback(string path);

    public class FileBrowser : MonoBehaviour
    {
        public string defaultPath = "";
        public bool selectDirectory = false;
        public bool showFiles = false;
        public bool canCancel = true;
        public string fileFormat = "";
        public bool searchCaseSensitive = false;

        public GameObject overlay;
        public GameObject window;
        public GameObject fileButtonPrefab;
        public GameObject directoryButtonPrefab;
        public RectTransform fileContent;
        public RectTransform dirContent;

        public InputField currentPathField;
        public InputField searchField;
        public Button searchCancelButton;
        public Button cancelButton;

        public Sprite folderIcon;
        public Sprite defaultIcon;
        public List<FileIcon> fileIcons = new List<FileIcon>();

        [SerializeField][HideInInspector]
        string currentPath;
        [SerializeField]
        [HideInInspector]
        string search;
        [SerializeField][HideInInspector]
        string slash;
        [SerializeField][HideInInspector]
        List<string> drives; // windows drives
        List<FileButton> fileButtons;
        List<DirectoryButton> dirButtons; // directory tree buttons
        int selected = -1;
        FileBrowserCallback callback = null;

        /// <summary>
        /// Get the selected path, if no path was selected it will return null.
        /// </summary>
        public string SelectedPath
        {
            get
            {
                if (selected > -1)
                {
                    return fileButtons[selected].fullPath;
                }
                else return null;
            }
        }

        void Awake()
        {
            slash = Path.DirectorySeparatorChar.ToString();
            if (Application.platform == RuntimePlatform.Android)
            {
                if (string.IsNullOrEmpty(defaultPath))
                defaultPath = "/storage";
            } else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                drives = new List<string>(Directory.GetLogicalDrives());
            }
            else if (Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
            {
                if (string.IsNullOrEmpty(defaultPath))
                {
                    defaultPath = "/home/";
                }
            }
        }

        public void Show(FileBrowserCallback callback)
        {
            GotoDirectory(defaultPath);
            UpdateUI();
            this.callback = callback;
            if (overlay)
                overlay.SetActive(true);
            window.SetActive(true);
        }
        public void Hide()
        {
            if (selected > -1)
            {
                fileButtons[selected].Unselect();
            }
            currentPath = "";
            selected = -1;
            search = "";
            if (overlay)
                overlay.SetActive(false);
            window.SetActive(false);
        }

        public void UpdateUI()
        {
            if (cancelButton)
            {
                cancelButton.gameObject.SetActive(canCancel);
            }
            currentPathField.text = currentPath;
            searchField.text = search;
        }

        public void OnFileClick(int i)
        {
            if (i >= fileButtons.Count)
            {
                Debug.LogError("uFileBrowser: Button index is bigger than array, something went wrong.");
                return;
            }
            if (fileButtons[i].isDir)
            {
                if (!selectDirectory)
                    GotoDirectory(fileButtons[i].fullPath);
                else
                {
                    SelectFile(i);
                }
            } else
            {
                SelectFile(i);
            }
        }
        public void OnDirectoryClick(int i)
        {
            if (i >= dirButtons.Count)
            {
                Debug.LogError("uFileBrowser: Button index is bigger than array, something went wrong.");
                return;
            }
            GotoDirectory(dirButtons[i].fullPath);
        }

        void GotoDirectory(string path)
        {
            if (path == currentPath && path != string.Empty)
            {
                return;
            }
            if (string.IsNullOrEmpty(path))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    currentPath = "";
                } else if (Application.platform == RuntimePlatform.Android)
                {
                    currentPath = "/storage";
                } else
                {
                    currentPath = "/home/";
                }
            } else
            {
                if (!Directory.Exists(path))
                {
                    Debug.LogError("uFileBrowser: Directory doesn't exist:\n" + path);
                    return;
                }
                else currentPath = path;
            }
            if (currentPathField)
            {
                currentPathField.text = currentPath;
            }
            selected = -1;
            UpdateFileList();
            UpdateDirectoryList();
        }
        void SelectFile(int i)
        {
            if (i >= fileButtons.Count)
            {
                Debug.LogError("uFileBrowser: Selection index bigger than array.");
                return;
            }
            if (i == selected)
            {
                if (selectDirectory && fileButtons[i].isDir)
                {
                    GotoDirectory(fileButtons[i].fullPath);
                    return;
                }
            }
            if (!fileButtons[i].isDir && selectDirectory)
            {
                return;
            }
            if (selected != -1)
            {
                fileButtons[selected].Unselect();
            }
            selected = i;
            fileButtons[i].Select();
        }

        void UpdateFileList()
        {
            if (fileButtons == null)
            {
                fileButtons = new List<FileButton>();
            } else
            {
                for (int i = 0; i < fileButtons.Count; i++)
                {
                    Destroy(fileButtons[i].gameObject);
                }
                fileButtons.Clear();
            }
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                if (string.IsNullOrEmpty(currentPath)) // display drives
                {
                    for (int i = 0; i < drives.Count; i++)
                    {
                        CreateFileButton(drives[i], drives[i], true, i);
                    }
                    return;
                }
            }
            List<string> files = new List<string>();
            if ((selectDirectory && showFiles) || (!selectDirectory))
            {
                try
                {
                    files = new List<string>(Directory.GetFiles(currentPath));
                } catch (System.Exception e)
                {
                    Debug.LogError("uFileBrowser: " + e.Message);
                }
                FilterFormat(files);
            }
            List<string> dirs = new List<string>();
            try
            {
                dirs = new List<string>(Directory.GetDirectories(currentPath));
            } catch (System.Exception e)
            {
                Debug.LogError("uFileBrowser: " + e.Message);
            }
            for (int i = 0; i < dirs.Count; i++)
            {
                string name = dirs[i].Substring(dirs[i].LastIndexOf(slash) + 1);
                CreateFileButton(name, dirs[i], true, fileButtons.Count);
            }
            for (int i = 0; i < files.Count; i++)
            {
                string name = files[i].Substring(files[i].LastIndexOf(slash) + 1);
                CreateFileButton(name, files[i], false, fileButtons.Count);
            }
        }
        void UpdateDirectoryList()
        {
            if (!directoryButtonPrefab)
            {
                return;
            }
            if (dirButtons == null)
            {
                dirButtons = new List<DirectoryButton>();
            } else
            {
                for (int i = 0; i < dirButtons.Count; i++)
                {
                    Destroy(dirButtons[i].gameObject);
                }
                dirButtons.Clear();
            }
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                CreateDirectoryButton("My Computer", "", 0);
            } else
            {
                CreateDirectoryButton(slash, slash, 0);
            }
            if (!string.IsNullOrEmpty(currentPath))
            {
                string[] dirs = currentPath.Split(slash[0]);
                for (int i = 0; i < dirs.Length; i++)
                {
                    if (!string.IsNullOrEmpty(dirs[i]))
                    {
                        string path = currentPath.Substring(0, currentPath.LastIndexOf(dirs[i]));
                        CreateDirectoryButton(dirs[i] + slash, path + dirs[i] + slash, dirButtons.Count);
                    }
                }
            }
        }
        void FilterFormat(List<string> files)
        {
            if (string.IsNullOrEmpty(fileFormat))
                return;
            string[] formats = fileFormat.Split('|');
            for (int i = 0; i < files.Count; i++)
            {
                bool remove = true;
                string extension = "";
                if (files[i].Contains("."))
                {
                    extension = files[i].Substring(files[i].LastIndexOf('.') + 1).ToLowerInvariant();
                }
                for (int j = 0; j < formats.Length; j++)
                {
                    if (extension == formats[j].Trim().ToLowerInvariant())
                    {
                        remove = false;
                        break;
                    }
                }
                if (remove)
                {
                    files.RemoveAt(i);
                    i--;
                }
            }
        }
        void FilterList()
        {
            if (string.IsNullOrEmpty(search))
            {
                for (int i = 0; i < fileButtons.Count; i++)
                {
                    fileButtons[i].gameObject.SetActive(true);
                }
                return;
            }
            for (int i = 0; i < fileButtons.Count;i++)
            {
                bool res = false;
                if (searchCaseSensitive)
                {
                    res = !fileButtons[i].text.Contains(search);
                } else
                {
                    res = !fileButtons[i].text.ToLowerInvariant().Contains(search.ToLowerInvariant());
                }
                if (res)
                {
                    fileButtons[i].gameObject.SetActive(false);
                }
                else fileButtons[i].gameObject.SetActive(true);
            }
        }

        void CreateFileButton(string text, string path, bool dir, int i)
        {
            GameObject obj = Instantiate(fileButtonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            obj.GetComponent<RectTransform>().SetParent(fileContent, false);
            FileButton fb = obj.GetComponent<FileButton>();
            fb.Set(this, text, path, dir, i);
            fileButtons.Add(fb);
        }
        void CreateDirectoryButton(string text, string path, int i)
        {
            GameObject obj = Instantiate(directoryButtonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            obj.GetComponent<RectTransform>().SetParent(dirContent, false);
            DirectoryButton db = obj.GetComponent<DirectoryButton>();
            db.Set(this, text, path, i);
            dirButtons.Add(db);
        }

        public void PathFieldEndEdit()
        {
            if (Directory.Exists(currentPathField.text))
            {
                GotoDirectory(currentPathField.text);
            }
            else currentPathField.text = currentPath;
        }
        public void SearchChanged()
        {
            if (!searchField)
                return;
            search = searchField.text.Trim();
            FilterList();
        }
        public void SearchCancelClick()
        {
            search = "";
            searchField.text = "";
            FilterList();
        }
        public void SelectButtonClicked()
        {
            if (selected > -1)
            {
                if ((fileButtons[selected].isDir && selectDirectory) || (!fileButtons[selected].isDir && !selectDirectory))
                {
                    callback(fileButtons[selected].fullPath);
                    Hide();
                }
            }
        }
        public void CancelButtonClicked()
        {
            if (!canCancel)
                return;
            if (callback != null)
            {
                callback("");
            }
            Hide();
        }

        public Sprite GetFileIcon(string path)
        {
            string extension = "";
            if (path.Contains("."))
            {
                extension = path.Substring(path.LastIndexOf('.') + 1);
            }
            else return defaultIcon;
            for (int i = 0; i < fileIcons.Count; i++)
            {
                if (fileIcons[i].extension == extension)
                {
                    return fileIcons[i].icon;
                }
            }
            return defaultIcon;
        }
    }
}

*/