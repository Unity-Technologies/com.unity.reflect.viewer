namespace SharpFlux.Dispatching
{
    public static class DispatcherFactory
    {
        static IDispatcher m_Dispatcher;
        public static IDispatcher GetDispatcher()
        {
            if (m_Dispatcher == null)
            {
                m_Dispatcher = new DispatcherImplementation();
                Dispatcher.RegisterDefaultDispatcher(m_Dispatcher);
            }

            return m_Dispatcher;
        }
    }
}
