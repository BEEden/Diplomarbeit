namespace HelixToolkit.SharpDX.Wpf
{
    using System.Linq;
    using System.Collections.Generic;
    using System.Collections;

    /// <summary>
    /// Provides a collection of Element3D.
    /// </summary>
    public class Element3DCollection : List<Element3D>
    {
        //internal void PreRenderSort()
        //{
        //    var comparer = new ElementComparer();
        //    this.Sort(comparer);
        //}
    }

    public class ElementComparer : IComparer
    {
        // Calls CaseInsensitiveComparer.Compare with the parameters reversed. 
        int IComparer.Compare(object x, object y)
        {
            return ((new CaseInsensitiveComparer()).Compare(y, x));
        }
    }
}