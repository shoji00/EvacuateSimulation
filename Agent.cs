using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvacuateSimulation_MultiAgent
{
    public class AgentBase
    {
        /// <summary>
        /// X座標
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y座標
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// エージェントの半径
        /// </summary>
        public double Radius { get; set; } = 25.0;

        /// <summary>
        /// エージェントの速度
        /// </summary>
        public double Speed { get; set; } = 15.0;

        public AgentBase(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
