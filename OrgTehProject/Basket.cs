//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OrgTehProject
{
    using System;
    using System.Collections.Generic;
    
    public partial class Basket
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Basket()
        {
            this.ItemInZakazs = new HashSet<ItemInZakaz>();
        }
    
        public int Id_Basket { get; set; }
        public int Id_User { get; set; }
        public int Id_Tehnika { get; set; }
        public int Quantity { get; set; }
        public Nullable<decimal> TotalPrice { get; set; }
        public bool IsContinued { get; set; }
    
        public virtual Tehnika Tehnika { get; set; }
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ItemInZakaz> ItemInZakazs { get; set; }
    }
}
