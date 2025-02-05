import yaml
from src.recognizer import RealTimeGestureRecognizer
from src.data_loader import SignLanguageDataLoader

def load_config():
    with open('configs/config.yaml', 'r') as f:
        return yaml.safe_load(f)

def main():
    # Load configuration
    config = load_config()
    
    # Initialize data loader to get label encoder
    data_loader = SignLanguageDataLoader(config)
    
    # Initialize recognizer
    model_path = f"{config['paths']['model_save_dir']}/welcome_gesture_model.h5"
    recognizer = RealTimeGestureRecognizer(
        model_path,
        data_loader.label_encoder,
        config
    )
    
    # Start recognition
    recognizer.run()

if __name__ == "__main__":
    main()