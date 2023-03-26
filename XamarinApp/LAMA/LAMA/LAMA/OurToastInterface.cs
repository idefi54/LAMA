using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA
{
    
    public interface ToastInterface
    {
        void DoTheThing(string message);
    }
    // USAGE
    // DependencyService.Get<ToastInterface>().DoTheThing(string message); 

}

