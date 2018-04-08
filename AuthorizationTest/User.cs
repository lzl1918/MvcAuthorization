using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizationTest
{
    public enum UserSexual
    {
        Male,
        Female,
        Other
    }

    public class User
    {
        public int Age { get; }
        public UserSexual Sexual { get; }
        public User(int age, UserSexual sexual)
        {
            Age = age;
            Sexual = sexual;
        }
    }

    public class Tree
    {
        public int Age { get; }
        public Tree(int age)
        {
            Age = age;
        }
    }
}
