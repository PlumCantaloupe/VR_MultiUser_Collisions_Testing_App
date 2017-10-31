using System;

[Serializable]
//public class UserObject {
//    public string id 		= "";
//    public float x 			= -1;
//	public float y 			= -1;
//	public float z 			= -1;
//	public int r 			= -1;
//	public int g 			= -1;
//	public int b 			= -1;
//	public string message 	= "";
//}

public class UserObject
{
    public string id            = "";

    public int[] color          = new int[3] {0,0,0};

    public float[] pos_root     = new float[3] { 0.0f, 0.0f, 0.0f };
    public float[] pos_head     = new float[3] { 0.0f, 0.0f, 0.0f };
    public float[] pos_hand_L   = new float[3] { 0.0f, 0.0f, 0.0f };
    public float[] pos_hand_R   = new float[3] { 0.0f, 0.0f, 0.0f };

    public float[] rot_root     = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };
    public float[] rot_head     = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };
    public float[] rot_hand_L   = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };
    public float[] rot_hand_R   = new float[4] { 0.0f, 0.0f, 0.0f, 0.0f };

    public string message       = "";
}

/*
 *     userObj.id = id;
    userObj.color = [r, g, b];

    userObj.pos_root = [root_pos_x, root_pos_y, root_pos_z];
    userObj.pos_head = [head_pos_x, head_pos_y, head_pos_z];
    userObj.pos_hand_L = [hand_L_pos_x, hand_L_pos_y, hand_L_pos_z];
    userObj.pos_hand_R = [hand_R_pos_x, hand_R_pos_y, hand_R_pos_z];

    userObj.rot_root = [root_rot_x, root_rot_y, root_rot_z, root_rot_w];
    userObj.rot_head = [head_rot_x, head_rot_y, head_rot_z, head_rot_w];
    userObj.rot_hand_L = [hand_L_rot_x, hand_L_rot_y, hand_L_rot_z, hand_L_rot_w];
    userObj.rot_hand_R = [hand_R_rot_x, hand_R_rot_y, hand_R_rot_z, hand_L_rot_w];

    userObj.message = message;
 */
