//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebTester.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class Token
    {
        public int id_Token { get; set; }
        public System.Guid UID { get; set; }
        public bool IsAvailable { get; set; }
        public System.DateTime dCreate { get; set; }
        public System.DateTime dExpire { get; set; }
        public string Description { get; set; }
    }
}