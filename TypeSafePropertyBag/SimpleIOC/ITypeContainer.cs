
/// <remarks>
/// This Simple inversion of control "Container" was copied whole sale from...
/// http://www.siepman.nl/blog/post/2014/02/15/Simple-IoC-container-easy-to-debug-and-fast.aspx
/// http://www.siepman.nl/blog/author/Admin.aspx
/// </remarks>

namespace DRM.TypeSafePropertyBag.Fundamentals.SimpleIOC
{
    interface ITypeContainer
    {
        void ForceCreate();
    }
}
