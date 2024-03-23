#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Plugin.DocuFlow.Documentation
{
    public class DocWindow : EditorWindow
    {
        private TextField searchField;
        private ScrollView leftScrollView;
        private ScrollView rightScrollView;
        private ListView nameList;
        private Label classNameLabel;
        private Label description;
        private Label fieldsHeading;
        private Label methodsHeading;
        private VisualElement propertiesList;
        private VisualElement methodsList;
        private ClassDocumentationData selectedData;
        private Label selectedLabel = null;
        private List<ClassDocumentationData> filteredDocumentation;
        private Stack<ClassDocumentationData> history = new Stack<ClassDocumentationData>();
        private Dictionary<string, Label> classLabels = new Dictionary<string, Label>();

        [MenuItem("Window/Documentation")]
        public static void ShowWindow() => GetWindow<DocWindow>("Documentation");

        public async void OnEnable()
        {
            SetupUIElements();
            await PopulateNameList();
            await RefreshDocumentationAsync();
            SearchManager.BuildTrie(DocumentationManager.Documentation);
            AddButtonListener();
        }

        private void SetupUIElements()
        {
            SetupUXML();
            SetupStyleSheet();
            GetUIElements();
            ConfigureElementsStyle();
            RegisterSearchFieldCallback();
        }

        private void SetupUXML()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Plugin/DocuFlow/Documentation/UI/DocumentationWindow.uxml");
            if (visualTree == null) Debug.LogError("UXML file not found. Make sure the path is correct.");
            else visualTree.CloneTree(rootVisualElement);
        }

        private void SetupStyleSheet()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Plugin/DocuFlow/Documentation/UI/WindowStyle.uss");
            if (styleSheet == null) Debug.LogError("USS file not found. Make sure the path is correct.");
            else rootVisualElement.styleSheets.Add(styleSheet);
        }
    
        private void AddButtonListener()
        {
            Button backButton = rootVisualElement.Q<Button>("back-button");
            if (backButton != null)
            {
                backButton.clicked += NavigateBack;
            }
        }

        private void GetUIElements()
        {
            searchField = rootVisualElement.Q<TextField>("search-query");
            leftScrollView = rootVisualElement.Q<ScrollView>("left-scroll-view");
            rightScrollView = rootVisualElement.Q<ScrollView>("right-scroll-view");
            nameList = leftScrollView.Q<ListView>("name-list");
            classNameLabel = rightScrollView.Q<Label>("class-name");
            description = rightScrollView.Q<Label>("class-description");
            propertiesList = rightScrollView.Q<VisualElement>("properties-list");
            methodsList = rightScrollView.Q<VisualElement>("methods-list");
            fieldsHeading = rightScrollView.Q<Label>("fields-heading");
            methodsHeading = rightScrollView.Q<Label>("methods-heading");
        }

        private void ConfigureElementsStyle()
        {
            leftScrollView.style.flexBasis = 300;
            rightScrollView.style.flexDirection = FlexDirection.Row;
            rightScrollView.style.flexGrow = 1;
        }

        private void RegisterSearchFieldCallback() => searchField.RegisterValueChangedCallback(evt => FilterNameList(evt.newValue));

        private async Task PopulateNameList()
        {
            if (DocumentationManager.Documentation == null) await DocumentationManager.RefreshDocumentationAsync();
            filteredDocumentation = DocumentationManager.Documentation.ToList();
            foreach (var classDoc in filteredDocumentation) AddLabelToNameList(classDoc);
            if (filteredDocumentation.Any()) SelectData(filteredDocumentation.First());
        }

        private void AddLabelToNameList(ClassDocumentationData classDoc)
        {
            var classNameLabel = new Label(classDoc.ClassType.Name) {name = classDoc.ClassType.Name};
            classNameLabel.AddToClassList("class-name-label");
            classNameLabel.AddManipulator(new Clickable(() => SelectData(classDoc)));
            nameList.hierarchy.Add(classNameLabel);
            classLabels[classDoc.ClassType.Name] = classNameLabel; 
        }
    
        private async Task RefreshDocumentationAsync()
        {
            await DocumentationManager.RefreshDocumentationAsync();
            RefreshDocumentation();
        }

        private void RefreshDocumentation()
        {
            ClearPanes();
            if (selectedData == null) return;
            PopulateDocumentationPanes();
        }

        private void ClearPanes()
        {
            classNameLabel.text = "";
            description.text = "";
            propertiesList.Clear();
            methodsList.Clear();
        }

        private void PopulateDocumentationPanes()
        {
            classNameLabel.text = selectedData.ClassType.Name;
            description.text = selectedData.Description;

            if (selectedData.PropertiesData.Any())
            {
                ShowElement(propertiesList);
                ShowElement(fieldsHeading);
                foreach (var propertyData in selectedData.PropertiesData) AddPropertyToPropertiesList(propertyData);
            }
            else
            {
                HideElement(propertiesList);
                HideElement(fieldsHeading);
            }

            if (selectedData.MethodsData.Any())
            {
                ShowElement(methodsList);
                ShowElement(methodsHeading);
                foreach (var methodData in selectedData.MethodsData) AddMethodToMethodsList(methodData);
            }
            else
            {
                HideElement(methodsList);
                HideElement(methodsHeading);
            }
        }
    
        private void HideElement(VisualElement element)
        {
            element.style.display = DisplayStyle.None;
        }

        private void ShowElement(VisualElement element)
        {
            element.style.display = DisplayStyle.Flex;
        }
    
        private void AddPropertyToPropertiesList(PropertyDocumentationData propertyData)
        {
            var propertyLabel = CreateLabel(propertyData.Property.Name, "header");
            propertyLabel.AddManipulator(new Clickable(() => OpenScriptAtLine(propertyData.Property)));
            var propertyDescription = CreateLabel(propertyData.Description, "description");
            propertiesList.Add(propertyLabel);
            propertiesList.Add(propertyDescription);
        }

        private Label CreateLabel(string text, string styleClass)
        {
            var label = new Label(text);
            label.AddToClassList(styleClass);
            return label;
        }

        private void AddMethodToMethodsList(MethodDocumentationData methodData)
        {
            var methodLabel = CreateLabel(methodData.Method.Name, "header");
            methodLabel.AddManipulator(new Clickable(() => OpenScriptAtLine(methodData.Method)));
            var methodDescription = CreateLabel(methodData.Description, "description");
            methodsList.Add(methodLabel);
            methodsList.Add(methodDescription);
        }

        private void OpenScriptAtLine(MemberInfo member)
        {
            var allScriptAssets = AssetDatabase.FindAssets("t:script");
            string memberClassName = member.DeclaringType.Name;

            TextAsset targetAsset = allScriptAssets
                .Select(scriptAssetGUID => AssetDatabase.GUIDToAssetPath(scriptAssetGUID))
                .Where(scriptAssetPath => Path.GetFileNameWithoutExtension(scriptAssetPath) == memberClassName)
                .Select(scriptAssetPath => AssetDatabase.LoadAssetAtPath<TextAsset>(scriptAssetPath))
                .FirstOrDefault();

            if (targetAsset == null) Debug.LogError($"Failed to find script file for class {memberClassName}");
            else AssetDatabase.OpenAsset(targetAsset);
        }

        private void SelectData(ClassDocumentationData data)
        {
            UnhighlightLabel();
            selectedLabel = FindLabelForSelectedData(data);
            if (selectedLabel != null) selectedLabel.AddToClassList("selected");
            PushToHistory();
            selectedData = data;
            RefreshDocumentation();
        }

        private void UnhighlightLabel()
        {
            if (selectedLabel != null) selectedLabel.RemoveFromClassList("selected");
        }

        private Label FindLabelForSelectedData(ClassDocumentationData data) => nameList.Q<Label>(data.ClassType.Name);

        private void PushToHistory()
        {
            if (selectedData != null) history.Push(selectedData);
        }

        private void FilterNameList(string query)
        {
            filteredDocumentation = SearchManager.FilterAndScoreDocumentation(query, DocumentationManager.Documentation);
            ClearAndRepopulateNameList();
            ManageSelectedDataAfterFiltering();
        }

        private void ClearAndRepopulateNameList()
        {
            nameList.hierarchy.Clear();
            foreach (var classDoc in filteredDocumentation)
            {
                if (classLabels.TryGetValue(classDoc.ClassType.Name, out var label))
                {
                    nameList.hierarchy.Add(label);
                }
                else
                {
                    Debug.LogError($"Failed to find cached label for {classDoc.ClassType.Name}");
                }
            }
        }

        private void ManageSelectedDataAfterFiltering()
        {
            UnhighlightLabel();
            if (!filteredDocumentation.Contains(selectedData)) SelectFirstDataInFilteredDocumentation();
            else HighlightLabelInFilteredDocumentation();
        }

        private void SelectFirstDataInFilteredDocumentation()
        {
            if (filteredDocumentation.Count > 0)
            {
                SelectData(filteredDocumentation[0]);
                HighlightFirstLabel();
            }
            else ClearSelectedDataAndLabel();
        }

        private void HighlightFirstLabel()
        {
            if (!nameList.Children().Any()) return;
            selectedLabel = nameList.Children().First() as Label;
            selectedLabel?.AddToClassList("selected");
        }

        private void ClearSelectedDataAndLabel()
        {
            selectedData = null;
            selectedLabel = null;
        }

        private void HighlightLabelInFilteredDocumentation()
        {
            selectedLabel = nameList.Q<Label>(selectedData.ClassType.Name);
            selectedLabel?.AddToClassList("selected");
        }

        private void NavigateBack()
        {
            if (history.Count > 0)
            {
                ClassDocumentationData lastViewed = history.Pop();
                SelectData(lastViewed);
            }
        }

        [DidReloadScripts]
        private static async void OnScriptsReloaded()
        {
            await DocumentationManager.RefreshDocumentationAsync();
            SearchManager.BuildTrie(DocumentationManager.Documentation);
            var windows = Resources.FindObjectsOfTypeAll<DocWindow>();
            foreach (DocWindow window in windows) window.RefreshDocumentation();
        }

    }
}

#endif