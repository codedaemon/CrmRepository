﻿using System;

namespace CrmRepository.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int Age { get; set; }
        public string SecretAgentId { get; set; }
    }
}