﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DRM.TypeSafePropertyBag.Fundamentals
{
    public static class CollectionViewExtensions
    {
        /// <summary>
        /// Casts a System.ComponentModel.ICollectionView of as a System.Collections.Generic.List&lt;T&gt; of the specified type.
        /// </summary>
        /// <typeparam name="TResult">The type to cast the elements of <paramref name="source"/> to.</typeparam>
        /// <param name="source">The System.ComponentModel.ICollectionView that needs to be casted to a System.Collections.Generic.List&lt;T&gt; of the specified type.</param>
        /// <returns>A System.Collections.Generic.List&lt;T&gt; that contains each element of the <paramref name="source"/>
        /// sequence cast to the specified type.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidCastException">An element in the sequence cannot be cast to the type <typeparamref name="TResult"/>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Method is provided for convenience.")]
        public static List<TResult> AsList<TResult>(this ICollectionView source)
        {
            return source.Cast<TResult>().ToList();
        }
    }
}
