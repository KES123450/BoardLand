using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;

namespace Cardinals.Game
{
    public class DiceDataEditor : OdinMenuEditorWindow
    {
        private CreateNewDiceData _createNewDiceData;

        [MenuItem("Tools/주사위 데이터 편집")]
        private static void OpenWindow()
        {
            GetWindow<DiceDataEditor>().Show();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_createNewDiceData != null)
            {
                _createNewDiceData.DestoryData();
            }
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();

            _createNewDiceData = new CreateNewDiceData();
            tree.Add("새 주사위", _createNewDiceData);

            tree.AddAllAssetsAtPath(
                "쥬사위 리스트",
                Constants.FilePath.Resources.Get(
                    Constants.FilePath.Resources.SO +
                    Constants.FilePath.Resources.SO_DiceData
                ),
                typeof(DiceDataSO)
            );

            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            OdinMenuTreeSelection selection = this.MenuTree.Selection;

            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUILayout.FlexibleSpace();

                if (SirenixEditorGUI.ToolbarButton("삭제"))
                {
                    DiceDataSO DiceData = (DiceDataSO)selection.SelectedValue;
                    string path = AssetDatabase.GetAssetPath(DiceData);
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.SaveAssets();
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        public class CreateNewDiceData
        {
            [Title("주사위 설정")]
            [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
            [ShowInInspector] private DiceDataSO _DiceData;

            public CreateNewDiceData()
            {
                _DiceData = ScriptableObject.CreateInstance<DiceDataSO>();
            }

            public void DestoryData()
            {
                if (_DiceData != null)
                {
                    DestroyImmediate(_DiceData);
                }
            }

            [Button("생성", ButtonSizes.Large)]
            private void Create()
            {
                AssetDatabase.CreateAsset(
                    _DiceData,
                    Constants.FilePath.Resources.Get(
                        Constants.FilePath.Resources.SO +
                        Constants.FilePath.Resources.SO_DiceData +
                        _DiceData.diceType.ToString() + ".asset"
                    )
                );
                AssetDatabase.SaveAssets();
            }
        }
    }
}
#endif