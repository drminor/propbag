using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRM.PropBag.AutoMapperSupport
{
    public enum PropBagMappingStrategyEnum
    {
        /// <summary>
        /// A new object of a Type that inherits from the original ViewModel Type
        /// is used in placed of the original ViewModel instance.
        /// </summary>
        EmitProxy,

        /// <summary>
        /// A object is created that wrapps the original ViewModel instance.
        /// When this wrapper is accessed, it causes the original ViewModel to be updated.
        /// </summary>
        EmitWrapper,

        /// <summary>
        /// AutoMapper is given a list of additional PropertyInfo (System.Reflection) objects.
        /// These PropertyInfo objects are used to update the original ViewModel instance.
        /// Note: This requires a custom version of AutoMapper.
        /// </summary>
        ExtraMembers
    }
}
