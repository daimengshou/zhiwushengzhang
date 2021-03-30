using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface ITreeParams
{
    int      GrowthCycle            { get;}
    double   Biomass                { get;}
    double   AbovegroundBiomass     { get;}
    double   Height                 { get;}
    double   LeafArea               { get;}
}

