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
    
    public partial class Tehnika
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tehnika()
        {
            this.Baskets = new HashSet<Basket>();
        }
    
        public int Id_Tehnika { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int Id_CategoryOfTehnika { get; set; }
        public int Id_TypeOfTehnika { get; set; }
        public int Id_CountryForMade { get; set; }
        public string Image { get; set; }
        public Nullable<bool> IsEnabel { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Basket> Baskets { get; set; }
        public virtual CategoryOfTehnika CategoryOfTehnika { get; set; }
        public virtual CountryForMade CountryForMade { get; set; }
        public virtual TypeTehnika TypeTehnika { get; set; }
    }
}
