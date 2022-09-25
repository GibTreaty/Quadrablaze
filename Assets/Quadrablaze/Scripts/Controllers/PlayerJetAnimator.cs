using Quadrablaze.Effects;
using UnityEngine;

public class PlayerJetAnimator : StateMachineBehaviour {

    public string jetTransformPath;
    public string boolProperty;

    JetParticles jetParticles;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if(!jetParticles) {
            var jetTransform = animator.transform.Find(jetTransformPath);

            if(jetTransform)
                jetParticles = jetTransform.GetComponentInChildren<JetParticles>();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if(jetParticles && animator.GetBool(boolProperty) != jetParticles.Opened)
            animator.SetBool(boolProperty, jetParticles.Opened);
    }
}