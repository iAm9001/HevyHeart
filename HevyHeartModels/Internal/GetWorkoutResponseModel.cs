using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevyHeartModels.Internal
{
    /// <summary>
    /// Represents a combined model containing both V1 and V2 workout response data.
    /// This class is used to hold workout information from different API versions for comparison,
    /// migration, or unified processing purposes.
    /// </summary>
    public class GetWorkoutResponseModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetWorkoutResponseModel"/> class.
        /// </summary>
        /// <param name="getWorkoutResponseV1">The V1 workout response data. Cannot be null.</param>
        /// <param name="getWorkoutResponseV2">The V2 workout response data. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when either parameter is null.</exception>
        public GetWorkoutResponseModel(HevyHeartModels.Hevy.V1.GetWorkoutResponse getWorkoutResponseV1,
            HevyHeartModels.Hevy.V2.GetWorkoutResponse getWorkoutResponseV2)
        {
            GetWorkoutResponseV1 =
                getWorkoutResponseV1 ?? throw new ArgumentNullException(nameof(getWorkoutResponseV1));
            GetWorkoutResponseV2 =
                getWorkoutResponseV2 ?? throw new ArgumentNullException(nameof(getWorkoutResponseV2));
        }

        /// <summary>
        /// Gets the V1 workout response data.
        /// </summary>
        public HevyHeartModels.Hevy.V1.GetWorkoutResponse GetWorkoutResponseV1 { get; }

        /// <summary>
        /// Gets the V2 workout response data.
        /// </summary>
        public HevyHeartModels.Hevy.V2.GetWorkoutResponse GetWorkoutResponseV2 { get; }
    }
}
