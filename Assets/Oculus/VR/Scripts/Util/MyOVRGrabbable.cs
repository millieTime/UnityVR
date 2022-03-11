/************************************************************************************
Copyright : Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.

Your use of this SDK or tool is subject to the Oculus SDK License Agreement, available at
https://developer.oculus.com/licenses/oculussdk/

Unless required by applicable law or agreed to in writing, the Utilities SDK distributed
under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF
ANY KIND, either express or implied. See the License for the specific language governing
permissions and limitations under the License.
************************************************************************************/

using System;
using UnityEngine;

/// <summary>
/// An object that can be grabbed and thrown by MyOVRGrabber.
/// </summary>
public class MyOVRGrabbable : MonoBehaviour
{
    [SerializeField]
    protected bool m_allowOffhandGrab = true;
    [SerializeField]
    protected bool m_snapPosition = false;
    [SerializeField]
    protected bool m_snapOrientation = false;
    [SerializeField]
    protected Transform m_snapOffset;
    [SerializeField]
    protected Collider[] m_grabPoints = null;

    protected bool m_grabbedKinematic = false;
    protected Collider m_grabbedCollider = null;
    protected MyOVRGrabber m_grabbedBy = null;

    //MODIFIED
    protected Vector3?[] velocityFrames;
    protected Vector3?[] angularVelocityFrames;
    protected int currentVelocityFrameStep = 0;
    protected Rigidbody rb;
    protected Vector3 PrevPos;
    protected Vector3 NewPos;

	/// <summary>
	/// If true, the object can currently be grabbed.
	/// </summary>
    public bool allowOffhandGrab
    {
        get { return m_allowOffhandGrab; }
    }

	/// <summary>
	/// If true, the object is currently grabbed.
	/// </summary>
    public bool isGrabbed
    {
        get { return m_grabbedBy != null; }
    }

	/// <summary>
	/// If true, the object's position will snap to match snapOffset when grabbed.
	/// </summary>
    public bool snapPosition
    {
        get { return m_snapPosition; }
    }

	/// <summary>
	/// If true, the object's orientation will snap to match snapOffset when grabbed.
	/// </summary>
    public bool snapOrientation
    {
        get { return m_snapOrientation; }
    }

	/// <summary>
	/// An offset relative to the MyOVRGrabber where this object can snap when grabbed.
	/// </summary>
    public Transform snapOffset
    {
        get { return m_snapOffset; }
    }

	/// <summary>
	/// Returns the MyOVRGrabber currently grabbing this object.
	/// </summary>
    public MyOVRGrabber grabbedBy
    {
        get { return m_grabbedBy; }
    }

	/// <summary>
	/// The transform at which this object was grabbed.
	/// </summary>
    public Transform grabbedTransform
    {
        get { return m_grabbedCollider.transform; }
    }

	/// <summary>
	/// The Rigidbody of the collider that was used to grab this object.
	/// </summary>
    public Rigidbody grabbedRigidbody
    {
        get { return m_grabbedCollider.attachedRigidbody; }
    }

	/// <summary>
	/// The contact point(s) where the object was grabbed.
	/// </summary>
    public Collider[] grabPoints
    {
        get { return m_grabPoints; }
    }

	/// <summary>
	/// Notifies the object that it has been grabbed.
	/// </summary>
	virtual public void GrabBegin(MyOVRGrabber hand, Collider grabPoint)
    {
        m_grabbedBy = hand;
        m_grabbedCollider = grabPoint;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
    }

	/// <summary>
	/// Notifies the object that it has been released.
	/// </summary>
	virtual public void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity, Vector3 cross)
    {
        rb = gameObject.GetComponent<Rigidbody>();
	  // MODIFIED
	  Vector3 fullThrowVelocity = linearVelocity + cross;

        rb.isKinematic = m_grabbedKinematic;

        rb.velocity = fullThrowVelocity;
        rb.angularVelocity = angularVelocity;
	  AddVelocityHistory();
	  ResetVelocityHistory();

        m_grabbedBy = null;
        m_grabbedCollider = null;
    }


    void FixedUpdate()
    {
	  NewPos = rb.transform.position;
        VelocityUpdate((NewPos - PrevPos) / Time.fixedDeltaTime);
	  PrevPos = NewPos;
    }
    void VelocityUpdate(Vector3 currentVelocity)
    {
        if (velocityFrames != null)
        {
	      // increment the current frame step
		currentVelocityFrameStep++;
		// if the current frame index is greater than the max number of steps
		if (currentVelocityFrameStep >= velocityFrames.Length)
		{
		    // reset steps when it goes over the value
		    currentVelocityFrameStep = 0;
		}
		// set the velocity at the current frame step to equal the current velocity and angulare velocity
		velocityFrames[currentVelocityFrameStep] = currentVelocity;
		angularVelocityFrames[currentVelocityFrameStep] = rb.angularVelocity;
        }
    }
    void AddVelocityHistory()
    {
	  if (velocityFrames != null)
	  {
		// get the average vector from our saved frames' velocities
		Vector3? velocityAverage = GetVectorAverage(velocityFrames);
		if (velocityAverage != null)
		{
		    //if our average isn't 0, apply it to the rigidbody
		    rb.velocity = velocityAverage?? rb.velocity;
		}
		// do the same to angular velocity
		Vector3? angularVelocityAverage = GetVectorAverage(angularVelocityFrames);
		if (angularVelocityAverage != null)
		{
		    rb.angularVelocity = angularVelocityAverage?? rb.angularVelocity;
		}
	  }
    }
    void ResetVelocityHistory()
    {
	  // first reset the current step to 0
	  currentVelocityFrameStep = 0;
	  // prevent nulls
	  if (velocityFrames != null && velocityFrames.Length > 0)
	  {
		// reset the frame step arrays by reinitializing
		velocityFrames = new Vector3?[velocityFrames.Length];
		angularVelocityFrames = new Vector3?[velocityFrames.Length];
	  }
    }
    Vector3? GetVectorAverage(Vector3?[] vectors)
    {
        //floats to store the positional data within
	  float x = 0f, y = 0f, z = 0f;

	  // how many vectors we have; we will divide by this
	  int numVectors = 0;

	  // run through our positions
	  for (int i = 0; i < vectors.Length; i++)
	  {
		if (vectors[i] != null)
		{
			// add the current vector's values to the running totals.
			x += vectors[i].Value.x;
			y += vectors[i].Value.y;
			z += vectors[i].Value.z;
			//increment the number of vectors we have
			numVectors++;
		}
	  }
    	  if (numVectors > 0)
    	  {
		// Get our average, only if numVectors isn't null
		Vector3 average = new Vector3(x / numVectors, y / numVectors, z / numVectors);
		return average;
	  }
    return null;
    }

    void Awake()
    {
        if (m_grabPoints.Length == 0)
        {
            // Get the collider from the grabbable
            Collider collider = this.GetComponent<Collider>();
            if (collider == null)
            {
				throw new ArgumentException("Grabbables cannot have zero grab points and no collider -- please add a grab point or collider.");
            }

            // Create a default grab point
            m_grabPoints = new Collider[1] { collider };
        }
    }

    protected virtual void Start()
    {
        m_grabbedKinematic = GetComponent<Rigidbody>().isKinematic;
        rb = gameObject.GetComponent<Rigidbody>();
        velocityFrames = new Vector3?[5];
	  angularVelocityFrames = new Vector3?[velocityFrames.Length];
	  PrevPos = rb.transform.position;
	  NewPos = rb.transform.position;
    }

    void OnDestroy()
    {
        if (m_grabbedBy != null)
        {
            // Notify the hand to release destroyed grabbables
            m_grabbedBy.ForceRelease(this);
        }
    }
}
