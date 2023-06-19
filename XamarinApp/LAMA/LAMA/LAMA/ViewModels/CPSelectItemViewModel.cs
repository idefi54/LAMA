using LAMA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace LAMA.ViewModels
{
    /// <summary>
    /// CP for the purposes of <see cref="CPSelectionViewModel"/>
    /// </summary>
    public class CPSelectItemViewModel : BaseViewModel
    {
        /// <summary>
        /// read only property of the corresponding CP.
        /// </summary>
        public CP cp { get; private set; }

        /// <summary>
        /// Name of the CP.
        /// </summary>
        public string Name => cp.name;
        /// <summary>
        /// Nick of the CP.
        /// </summary>
        public string Nick => cp.nick;
        /// <summary>
        /// Combination of both name and nick of the CP.
        /// </summary>
        public string FullIdentifier => $"{cp.nick} - {cp.name}";
        /// <summary>
        /// List of all roles joined with a “,”.
        /// </summary>
        public string Roles => String.Join(", ", cp.roles);
        public CPSelectItemViewModel(CP cp)
        {
            this.cp = cp;
        }
    }
}
