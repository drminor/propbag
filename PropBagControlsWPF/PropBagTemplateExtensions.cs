using System;

using System.Windows.Controls;

using DRM.PropBag.ControlModel;

namespace DRM.PropBag.ControlsWPF
{
    public static class PropBagTemplateExtensions
    {
        static public PropModel GetPropBagModel(this DRM.PropBag.ControlsWPF.PropBagTemplate theModel)
        {
            string className = theModel.ClassName;
            string outputNamespace = theModel.OutPutNameSpace;
            bool deriveFromPubPropBag = theModel.DeriveFromPubPropBag;
            PropBagTypeSafetyMode typeSafetyMode = theModel.TypeSafetyMode;
            bool deferMethodRefResolution = theModel.DeferMethodRefResolution;
            bool requireExplicitInitialValue = theModel.RequireExplicitInitialValue;

            PropModel result =
                new PropModel(className,
                    outputNamespace,
                    deriveFromPubPropBag,
                    typeSafetyMode,
                    deferMethodRefResolution,
                    requireExplicitInitialValue);


            int namespacesCount = theModel.Namespaces == null ? 0 : theModel.Namespaces.Count;
            for (int nsPtr = 0; nsPtr < namespacesCount; nsPtr++)
            {
                result.Namespaces.Add(theModel.Namespaces[nsPtr].Namespace);
            }

            int propsCount = theModel.Props == null ? 0 : theModel.Props.Count;

            for (int propPtr = 0; propPtr < propsCount; propPtr++)
            {
                PropItem pi = theModel.Props[propPtr];

                bool hasStore = pi.HasStore; 
                bool typeIsSolid = pi.TypeIsSolid; 
                string extraInfo = pi.ExtraInfo; 

                ControlModel.PropItem rpi = new ControlModel.PropItem(pi.PropertyType, pi.PropertyName, extraInfo, hasStore, typeIsSolid);

                foreach (Control uc in pi.Items)
                {
                    if (uc is InitialValueField)
                    {
                        InitialValueField ivf = (InitialValueField)uc;
                        rpi.InitialValueField = 
                            new ControlModel.PropInitialValueField(ivf.InitialValue, ivf.SetToDefault, ivf.SetToUndefined, ivf.SetToNull, ivf.SetToEmptyString);
                        continue;
                    }

                    if (uc is PropDoWhenChangedField)
                    {
                        PropDoWhenChangedField dwc = (PropDoWhenChangedField)uc;

                        ControlModel.PropDoWhenChangedField rdwc =
                            new ControlModel.PropDoWhenChangedField(dwc.DoWhenChangedAction.DoWhenChanged, dwc.DoAfterNotify);

                        rpi.DoWhenChangedField = rdwc; 
                        continue;
                    }

                    if (uc is PropComparerField)
                    {
                        PropComparerField pcf = (PropComparerField)uc;

                        ControlModel.PropComparerField rpcf =
                            new ControlModel.PropComparerField(pcf.ComparerFunc.Comparer, pcf.UseRefEquality);

                        rpi.ComparerField = rpcf;
                    }
                }

                result.Props.Add(rpi);

            }
            return result;
        }

    }
}
