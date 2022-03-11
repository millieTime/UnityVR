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
/// An object that will retrieve the ball when grabbed by MyOVRGrabber.
/// </summary>
public class MyAcquireBall : MyOVRGrabbable
{

    //MODIFIED
    [SerializeField]
    protected Rigidbody ballRB;

	/// <summary>
	/// Notifies the object that it has been grabbed.
	/// </summary>
	public override void GrabBegin(MyOVRGrabber hand, Collider grabPoint)
    {
        m_grabbedBy = hand;
        m_grabbedCollider = grabPoint;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
	  ballRB.position = new Vector3(rb.position.x, rb.position.y, rb.position.z);
	  ballRB.velocity = Vector3.zero;

    }

	/// <summary>
	/// Notifies the object that it has been released.
	/// </summary>
	public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity, Vector3 cross)
    {
        rb = gameObject.GetComponent<Rigidbody>();
	  // MODIFIED

        rb.isKinematic = m_grabbedKinematic;
	  rb.velocity = Vector3.zero;
        m_grabbedBy = null;
        m_grabbedCollider = null;
    }

    protected override void Start()
    {
        m_grabbedKinematic = GetComponent<Rigidbody>().isKinematic;
        rb = gameObject.GetComponent<Rigidbody>();
    }
}
