using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace kenzauros.RHarbor.MvvmDialog
{
    /// <summary>
    /// VM からダイアログウィンドウを生成
    /// </summary>
    public static class DialogFactory
    {
        public static Dictionary<Type, Type> DialogToWindowDictionary { get; } = new Dictionary<Type, Type>();

        public static void Register(Type vm, Type window)
        {
            if (!DialogToWindowDictionary.ContainsKey(vm))
            {
                DialogToWindowDictionary.Add(vm, window);
            }
        }

        public static Window Create(Type dialogType)
        {
            if (DialogToWindowDictionary.ContainsKey(dialogType))
            {
                var type = DialogToWindowDictionary[dialogType];
                return Activator.CreateInstance(type) as Window;
            }
            else
            {
                var expectedWindowNames = new[] {
                    dialogType.Name.Replace("WindowViewModel", "Window"),
                    dialogType.Name.Replace("ViewModel", "Window"),
                    dialogType.Name.Replace("WindowVM", "Window"),
                    dialogType.Name.Replace("VM", "Window"),
                    };
                var type = dialogType.Assembly.GetTypes()
                    .FirstOrDefault(x => expectedWindowNames.Contains(x.Name) && x.IsSubclassOf(typeof(Window)));
                if (type != null)
                {
                    DialogToWindowDictionary[dialogType] = type;
                    return Activator.CreateInstance(type) as Window;
                }

                throw new ArgumentException($"{dialogType.Name} に対応するウィンドウが定義されていません。");
            }
        }
    }
}
