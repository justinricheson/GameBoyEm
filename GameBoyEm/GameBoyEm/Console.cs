namespace GameBoyEm
{
    public class Console
    {
        private ICpu _cpu;

        public Console(ICpu cpu)
        {
            _cpu = cpu;
        }
    }
}
