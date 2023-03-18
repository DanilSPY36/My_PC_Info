using System;
using OpenHardwareMonitor.Hardware;

namespace PC_Info
{
    internal class Visitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware hw in hardware.SubHardware)
            {
                hw.Accept(this);
            }
        }
        public void VisitParameter(IParameter parameter) { }
        public void VisitSensor(ISensor sensor) { }
    }
}