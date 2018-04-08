using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AuthorizationCore;

namespace AuthorizationTest
{
    public class Startup
    {
        public Startup()
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSession();
            services.AddMvcAuthorization<User>(options =>
            {
                options.UserAccessor = provider => new User(24, UserSexual.Male);
                options.AddPolicy(new AgeGreaterThanPolicy(18), "IsAgeGreaterThan18");
                options.AddPolicy(new AgeGreaterThanPolicy(30), "IsAgeGreaterThan30");
                options.AddHandler<AgeGreaterThanPolicy, AgeGreaterThanPolicyHandler>();

                options.AddPolicy(new TreeGreaterThanPolicy(), provider => new Tree(30), "IsGreaterThanTree");
                options.AddHandler<Tree, TreeGreaterThanPolicy, TreeGreaterThanPolicyHandler>();
            });
            services.AddMvc();

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Test}/{action=Index}");
            });


        }
    }
}
