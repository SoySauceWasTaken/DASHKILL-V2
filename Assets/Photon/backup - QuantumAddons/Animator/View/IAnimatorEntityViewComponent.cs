namespace Quantum {

    public unsafe interface IAnimatorEntityViewComponent {
        void Init(Frame frame, AnimatorComponent* animator);
        void Animate(Frame frame, AnimatorComponent* animator);
        
    }
}
