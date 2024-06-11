using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Linq;


namespace BackToGame
{
    static public class Extension
    {
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject parent, string typeName) where T : DependencyObject
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            var queue = new Queue<DependencyObject>(new[] { parent });

            while (queue.Any())
            {
                var reference = queue.Dequeue();
                var count = VisualTreeHelper.GetChildrenCount(reference);

                for (var i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(reference, i);
                    if (child.GetType().FullName == typeName)
                        yield return child as T;

                    queue.Enqueue(child);
                }
            }
        }
    }
}
