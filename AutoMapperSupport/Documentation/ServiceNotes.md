# Markdown file

        #region NOTES

        //-------------------------

        // Other Dependencies that could be managed, but not part of AutoMapper, per say.
        // 1. Type Converter Cache used by most, if not all IPropFactory.
        // 2. DoSetDelegate Cache -- basically baked into IPropBag -- not many different options
        // 3. PropCreation Delegate Cache --  only used by IPropFactory this is not critical now, but could become so 
        // 4. Event Listeners that could managed better if done as a central service -- used by PropBag and the Binding engine.

        //------------------------ 

        //******************

        // Services that we need to focus on now.
        // 1. ViewModel creation 
        // 2. PropFactory boot strapping
        // 3. Proxy Model creation (we should be able to use 95% of the same services as those provided for ViewModel
        //          creation, in fact, ProxModel creation is probably the driver and ViewModel can benefit from
        //          novel techniqes explored here.
        // 4. Creating a language for ViewModel configuration.
        // 5. Creating services that allow for data flow behavior to be declared and executed without having
        //          to write code.
        // 6. Creating ViewModel sinks for data coming from the View dynamically, ReactiveUI has a 
        //          a way of doing this from the ViewModel to the View, can we build a facility to allow the reverse?
        // 
        // 7. Allowing the View to affect the behavior of the ViewModel dynamically.
        // 8. Design-time support including AutoMapper mapping configuration and testing.
        //******************

        // +++++++++++++++++++

        // Other services that should be addressed
        // 1. Building TypeDescriptors / Type Descriptions / PropertyInfo / Custom MetaData for reflection.
        // 2. XML Serialization services for saving / hydrating IPropBag objects.
        // 
        // +++++++++++++++++++

        #endregion
