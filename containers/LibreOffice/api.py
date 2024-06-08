from flask import Flask, request, jsonify, send_file
from flask_cors import CORS
import os


app = Flask(__name__)
CORS(app)

#endpoint that receives a file and returns the number of words in it
@app.route('/convert', methods=['POST'])
def wordcount():
    file = request.files['file']
    file.save(file.filename)
    
    os.system(f"libreoffice --headless --convert-to pdf {file.filename}")

    name = file.filename.replace(".docx", ".pdf")
    #return the file
    return send_file(name, as_attachment=True)

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)