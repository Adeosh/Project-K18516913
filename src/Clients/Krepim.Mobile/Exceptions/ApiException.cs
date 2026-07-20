using Krepim.Mobile.Http;

namespace Krepim.Mobile.Exceptions
{
    public class ApiException : Exception
    {
        public ProblemDetails Problem { get; }

        public ApiException(ProblemDetails problem)
            : base(problem.Detail ?? problem.Title)
        {
            Problem = problem;
        }
    }
}
