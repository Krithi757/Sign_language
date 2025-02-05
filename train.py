import os
import yaml
from src.data_loader import SignLanguageDataLoader
from src.model import SignLanguageModel
from sklearn.model_selection import train_test_split
import logging
from src.utils import setup_logging

def load_config():
    try:
        with open('configs/config.yaml', 'r') as f:
            return yaml.safe_load(f)
    except Exception as e:
        logging.error(f"Error loading config: {str(e)}")
        raise

def main():
    # Setup logging
    setup_logging()
    
    # Load configuration
    config = load_config()
    logging.info("Configuration loaded successfully")
    
    try:
        # Initialize data loader
        data_loader = SignLanguageDataLoader(config)
        logging.info("Data loader initialized")
        
        # Load and prepare data
        logging.info("Loading dataset...")
        X, y = data_loader.load_dataset(
            config['paths']['data_dir'],
            config['data']['gesture_name']
        )
        logging.info(f"Dataset loaded: {len(X)} samples")
        
        X_processed, y_processed = data_loader.prepare_data(X, y)
        logging.info("Data preprocessing completed")
        
        # Split data
        X_train, X_test, y_train, y_test = train_test_split(
            X_processed, y_processed,
            test_size=config['model']['validation_split'],
            random_state=42
        )
        logging.info("Data split completed")
        
        # Create and train model
        model = SignLanguageModel(config)
        input_shape = (X_processed.shape[1], X_processed.shape[2])
        model.build_model(input_shape, y_processed.shape[1])
        logging.info("Model built successfully")
        
        # Train
        logging.info("Starting model training...")
        history = model.train(
            X_train, y_train,
            X_test, y_test,
            epochs=config['model']['epochs'],
            batch_size=config['model']['batch_size']
        )
        
        # Save model
        os.makedirs(config['paths']['model_save_dir'], exist_ok=True)
        model_path = os.path.join(
            config['paths']['model_save_dir'],
            'welcome_gesture_model.h5'
        )
        model.save_model(model_path)
        logging.info(f"Model saved to {model_path}")
        
    except Exception as e:
        logging.error(f"Error during training: {str(e)}")
        raise

if __name__ == "__main__":
    main()