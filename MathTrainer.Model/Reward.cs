using System;

namespace MathTrainer.Model
{
    public class Reward
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public int Price { get; set; } = 1;

        public Reward()
        {
            Id = Guid.NewGuid();
        }
    }
}